using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Numerics;
using Rogue.Server.Adapter;
using Rogue.Server.Bridge;
using Rogue.Server.Builder;
using Rogue.Server.Combat;
using Rogue.Server.Enemies;
using Rogue.Server.Map;
using Rogue.Server.Observer;
using Rogue.Shared.Models;

namespace Rogue.Server.Game;

public class ConnectedClient
{
    public PlayerState State { get; }
    public SemaphoreSlim SendLock { get; } = new(1, 1);

    // Bridge: the attack pattern (abstraction) decides where the attack lands and
    // delegates damage, interval and reach to its weapon (implementor).
    // Adapter: that weapon is the external spear, used through the IWeapon interface.
    public AttackPattern Attack { get; set; } = new FrontalAttack(new SpearWeaponAdapter(new ThirdPartySpear()));

    public IWeapon Weapon
    {
        get => Attack.Weapon;
        set => Attack.Weapon = value;
    }
    public float AttackCooldown { get; set; } = 0f;
    public float DamageCooldown { get; set; } = 0f;

    public ConnectedClient(PlayerState state) => State = state;
}

public class GameServer : IGameSubject
{
    public ConcurrentDictionary<WebSocket, ConnectedClient> Clients { get; } = new();

    private readonly List<IGameObserver> _observers = new();
    private readonly object _observerLock = new();

    public int ObserverCount
    {
        get
        {
            lock (_observerLock) return _observers.Count;
        }
    }

    public void Attach(IGameObserver observer)
    {
        if (observer == null) throw new ArgumentNullException(nameof(observer));
        lock (_observerLock)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }
    }

    public void Detach(IGameObserver observer)
    {
        if (observer == null) return;
        lock (_observerLock)
        {
            _observers.Remove(observer);
        }
    }

    public async Task NotifyAsync()
    {
        List<IGameObserver> snapshot;
        lock (_observerLock)
        {
            snapshot = new List<IGameObserver>(_observers);
        }

        if (snapshot.Count == 0)
            return;

        var tasks = snapshot.Select(o => o.UpdateAsync(this));
        await Task.WhenAll(tasks);
    }

    private int nextId = 1;

    private readonly List<Tile> _tiles;

    // Factory builds the prototypes once; every spawned enemy is a clone of one.
    public EnemySpawner Spawner { get; } = new(new EnemyPrototypeRegistry(new EnemyFactory()));

    public GameServer()
    {
        // Builder: the director assembles the map step by step with a concrete builder.
        // A new random seed and theme on every server start gives a different cave each time.
        int seed = Random.Shared.Next();
        IMapThemeFactory theme = Random.Shared.Next(2) == 0 ? new WaterThemeFactory() : new LavaThemeFactory();
        Console.WriteLine($"Map: {theme.Name} cave, seed {seed}");

        var director = new MapDirector();
        GameMap map = director.BuildFullMap(new CaveMapBuilder(theme, seed),
                                            GameSettings.Instance.GridSize, spawnX: 3, spawnY: 3);
        _tiles = map.Tiles.ToList();

        // Stop enemies from spawning inside walls.
        Spawner.IsBlocked = IsBlocked;
    }

    public PlayerState AddPlayer(WebSocket socket)
    {
        PlayerState player = new()
        {
            Id = Interlocked.Increment(ref nextId) - 1,
            X = 3,
            Y = 3,
            Health = 100f,
            MaxHealth = 100f,
            FacingDx = 1f,
            FacingDy = 0f
        };

        Clients[socket] = new ConnectedClient(player);

        return player;
    }

    public void RemovePlayer(WebSocket socket)
    {
        Clients.TryRemove(socket, out _);
    }

    /// <summary>
    /// Advances the enemy spawner, moves enemies toward nearest player,
    /// executes player attacks, applies enemy collision damage, and removes dead enemies.
    /// </summary>
    public void Tick(float deltaTime)
    {
        Spawner.Update(deltaTime);

        var clients = Clients.Values.ToList();
        var players = clients.Select(c => c.State).ToList();
        if (players.Count == 0)
            return;

        // 1. Enemy Movement
        foreach (var enemy in Spawner.Enemies)
        {
            var target = players.MinBy(p => Vector2.DistanceSquared(enemy.Position, new Vector2(p.X, p.Y)))!;
            enemy.Update(new Vector2(target.X, target.Y), deltaTime);
        }

        // 2. Player Attacks (Bridge: attack pattern + weapon)
        foreach (var client in clients)
        {
            if (client.State.AttackAnimTimer > 0f)
            {
                client.State.AttackAnimTimer = MathF.Max(0f, client.State.AttackAnimTimer - deltaTime);
            }

            // Only attack if alive
            if (client.State.Health <= 0)
                continue;

            client.AttackCooldown -= deltaTime;
            if (client.AttackCooldown <= 0f)
            {
                // Reset cooldown using Interval. Ensures at most one attack per tick.
                client.AttackCooldown = client.Attack.Interval;
                client.State.AttackAnimTimer = 0.15f; // Active visual swing duration
                client.Attack.Execute(client.State, Spawner.Enemies);
            }
        }

        // 3. Enemy-to-Player Collision Damage
        const float CollisionRange = 0.8f;
        const float CollisionDamage = 10f;
        const float InvulnerabilityDuration = 1.0f;

        foreach (var client in clients)
        {
            if (client.State.Health <= 0)
                continue;

            if (client.DamageCooldown > 0f)
            {
                client.DamageCooldown = MathF.Max(0f, client.DamageCooldown - deltaTime);
            }

            if (client.DamageCooldown <= 0f)
            {
                Vector2 playerPos = new(client.State.X, client.State.Y);
                bool touchingEnemy = Spawner.Enemies.Any(e =>
                    e.Health > 0 &&
                    Vector2.Distance(playerPos, e.Position) <= CollisionRange);

                if (touchingEnemy)
                {
                    client.State.Health = MathF.Max(0f, client.State.Health - CollisionDamage);
                    client.DamageCooldown = InvulnerabilityDuration;
                }
            }
        }

        // 4. Authoritative Dead Enemy Removal
        // Gather dead IDs first to avoid modifying Spawner.Enemies during enumeration
        var deadEnemyIds = Spawner.Enemies
            .Where(e => e.Health <= 0f)
            .Select(e => e.Id)
            .ToList();

        foreach (var deadId in deadEnemyIds)
        {
            Spawner.RemoveEnemy(deadId);
        }
    }

    public List<PlayerState> GetSnapshot()
    {
        return Clients.Values.Select(c => c.State).ToList();
    }

    public List<EnemyState> GetEnemySnapshot() => Spawner.GetSnapshot();

    public List<TileState> GetTileSnapshot() => _tiles.Select(t => t.ToState()).ToList();

    public bool IsBlocked(int x, int y) =>
        _tiles.Any(t => t.IsBlocking && t.X == x && t.Y == y);
}