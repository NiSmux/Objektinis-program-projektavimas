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
    private readonly object _weaponLock = new();
    public PlayerState State { get; }
    public SemaphoreSlim SendLock { get; } = new(1, 1);

    // Bridge: the attack pattern (abstraction) decides where the attack lands and
    // delegates damage, interval and reach to its weapon (implementor).
    // Adapter: that weapon is the external spear, used through the IWeapon interface.
    public AttackPattern Attack { get; set; } = new FrontalAttack(new SpearWeaponAdapter(new ThirdPartySpear()));

    public IWeapon Weapon
    {
        get { lock (_weaponLock) return Attack.Weapon; }
        set { lock (_weaponLock) Attack.Weapon = value; }
    }
    public float AttackCooldown { get; set; } = 0f;
    public float DamageCooldown { get; set; } = 0f;

    public ConnectedClient(PlayerState state) => State = state;
}

public class GameServer : IGameSubject
{
    public ConcurrentDictionary<WebSocket, ConnectedClient> Clients { get; } = new();
    private readonly ConcurrentQueue<Action> _pendingActions = new();

    /// <summary>
    /// Enqueues a server action to be executed safely on the game loop tick.
    /// </summary>
    public void EnqueueAction(Action action) => _pendingActions.Enqueue(action);

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

        var client = new ConnectedClient(player);
        UpdatePlayerWeaponState(client);

        Clients[socket] = client;

        return player;
    }

    /// <summary>
    /// Applies a Decorator pattern upgrade to the player's weapon.
    /// </summary>
    public static void UpdatePlayerWeaponState(ConnectedClient client)
    {
        var weapon = client.Weapon;
        client.State.BaseWeaponName = weapon.Name;
        client.State.WeaponDamage = weapon.Damage;
        client.State.WeaponInterval = weapon.Interval;
        client.State.WeaponReach = weapon.Reach;
        client.State.WeaponHalfWidth = weapon.HalfWidth;

        if (weapon is WeaponDecorator decorator)
        {
            var applied = decorator.GetAppliedDecorators();
            client.State.WeaponName = applied.Count > 0
                ? $"{weapon.Name} (+{string.Join(", +", applied)})"
                : weapon.Name;
        }
        else
        {
            client.State.WeaponName = weapon.Name;
        }
    }

    /// <summary>
    /// Applies a Decorator pattern upgrade to the player's weapon with validation and limits.
    /// </summary>
    public bool UpgradeWeapon(WebSocket socket, string upgradeType)
    {
        if (!Clients.TryGetValue(socket, out var client))
            return false;

        switch (upgradeType.ToLowerInvariant())
        {
            case "speed":
                if (client.Weapon.Interval <= AttackSpeedDecorator.MinInterval + 0.001f)
                {
                    Console.WriteLine($"[Weapon Upgrade] Player {client.State.Id} speed upgrade skipped: already at minimum interval ({AttackSpeedDecorator.MinInterval:0.00}s).");
                    return false;
                }
                client.Weapon = new AttackSpeedDecorator(client.Weapon);
                break;

            case "damage":
                if (client.Weapon.Damage >= DamageBoostDecorator.MaxDamage - 0.001f)
                {
                    Console.WriteLine($"[Weapon Upgrade] Player {client.State.Id} damage upgrade skipped: already at max damage ({DamageBoostDecorator.MaxDamage:0.0}).");
                    return false;
                }
                client.Weapon = new DamageBoostDecorator(client.Weapon);
                break;

            case "range":
                if (client.Weapon.Reach >= RangeDecorator.MaxReach - 0.001f)
                {
                    Console.WriteLine($"[Weapon Upgrade] Player {client.State.Id} range upgrade skipped: already at max reach ({RangeDecorator.MaxReach:0.00}).");
                    return false;
                }
                client.Weapon = new RangeDecorator(client.Weapon);
                break;

            case "reset":
                client.Weapon = new WhipWeapon();
                break;

            default:
                return false;
        }

        UpdatePlayerWeaponState(client);

        Console.WriteLine($"[Weapon Upgrade] Player {client.State.Id} applied '{upgradeType}' -> " +
                          $"{client.State.WeaponName} " +
                          $"(Damage: {client.Weapon.Damage:0.0}, " +
                          $"Interval: {client.Weapon.Interval:0.00}s, " +
                          $"Reach: {client.Weapon.Reach:0.00}, " +
                          $"HalfWidth: {client.Weapon.HalfWidth:0.00})");
        return true;
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
        while (_pendingActions.TryDequeue(out var action))
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Error executing pending action: {ex.Message}");
            }
        }

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