using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Numerics;
using Rogue.Server.Combat;
using Rogue.Server.Enemies;
using Rogue.Server.Map;
using Rogue.Shared.Models;

namespace Rogue.Server.Game;

public class ConnectedClient
{
    public PlayerState State { get; }
    public SemaphoreSlim SendLock { get; } = new(1, 1);

    public IWeapon Weapon { get; set; } = new WhipWeapon();
    public float AttackCooldown { get; set; } = 0f;
    public float DamageCooldown { get; set; } = 0f;

    public ConnectedClient(PlayerState state) => State = state;
}

public class GameServer
{
    public ConcurrentDictionary<WebSocket, ConnectedClient> Clients { get; } = new();

    private int nextId = 1;

    private readonly List<Tile> _tiles;

    // Factory builds the prototypes once; every spawned enemy is a clone of one.
    public EnemySpawner Spawner { get; } = new(new EnemyPrototypeRegistry(new EnemyFactory()));

    public GameServer()
    {
        // Left half of the map is water, right half is lava.
        var generator = new MapGenerator(new WaterThemeFactory(), new LavaThemeFactory());
        _tiles = generator.Generate(9, spawnX: 3, spawnY: 3);

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

        // 2. Player Attacks (Directional Whip)
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
                client.AttackCooldown = client.Weapon.Interval;
                client.State.AttackAnimTimer = 0.15f; // Active visual swing duration
                ExecuteWhipAttack(client);
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

    /// <summary>
    /// Executes a directional whip attack for a player.
    /// Hits enemies in an oriented rectangular area in front of the player.
    /// Supports all 8 directions (including diagonals) using rotated coordinate projection.
    /// </summary>
    private void ExecuteWhipAttack(ConnectedClient client)
    {
        var player = client.State;
        var weapon = client.Weapon;

        Vector2 facing = new(player.FacingDx, player.FacingDy);
        if (facing.LengthSquared() < 0.0001f)
        {
            // Default facing if zero
            facing = new Vector2(1f, 0f);
        }
        else
        {
            facing = Vector2.Normalize(facing);
        }

        // Perpendicular vector for the width of the attack rectangle
        Vector2 perp = new(-facing.Y, facing.X);

        Vector2 playerPos = new(player.X, player.Y);

        // Track damaged enemies to prevent multiple hits to the same enemy in a single attack
        HashSet<int> hitEnemies = new();

        foreach (var enemy in Spawner.Enemies)
        {
            if (enemy.Health <= 0)
                continue;

            Vector2 toEnemy = enemy.Position - playerPos;

            // Project onto forward facing vector and perpendicular vector
            float forwardDist = Vector2.Dot(toEnemy, facing);
            float sideDist = MathF.Abs(Vector2.Dot(toEnemy, perp));

            // Enemy can be on top of player (within 0.7 radius) OR inside the forward reach rectangle
            float distSq = toEnemy.LengthSquared();
            bool onTopOfPlayer = distSq <= 0.7f * 0.7f;
            bool insideWhipRect = forwardDist >= -0.3f && forwardDist <= weapon.Reach && sideDist <= weapon.HalfWidth;

            if (onTopOfPlayer || insideWhipRect)
            {
                // HashSet ensures each individual enemy is hit once per swing even if overlapping both conditions
                // Multiple different enemies in range are all added and all receive damage
                if (hitEnemies.Add(enemy.Id))
                {
                    enemy.TakeDamage(weapon.Damage);
                }
            }
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