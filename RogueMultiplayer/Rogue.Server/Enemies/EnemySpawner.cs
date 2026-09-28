using Rogue.Shared.Models;

namespace Rogue.Server.Enemies;

/// <summary>
/// Manages timed enemy spawning using <see cref="IEnemyFactory"/>.
/// Call <see cref="Update"/> every game tick to automatically spawn enemies
/// at the configured interval.
/// </summary>
public class EnemySpawner
{
    private readonly IEnemyFactory _factory;
    private readonly List<Enemy> _enemies = new();
    private readonly Random _random = new();

    private int _nextId = 1;

    /// <summary>Grid size used when randomising spawn positions.</summary>
    public int GridSize { get; set; } = 9;

    /// <summary>Seconds between automatic spawns.</summary>
    public float SpawnInterval { get; set; } = 5f;

    /// <summary>Maximum number of enemies allowed on the map at once.</summary>
    public int MaxEnemies { get; set; } = 20;

    private float _timeSinceLastSpawn;

    public EnemySpawner(IEnemyFactory factory)
    {
        _factory = factory;
    }

    /// <summary>Read-only snapshot of currently active enemies.</summary>
    public IReadOnlyList<Enemy> Enemies => _enemies.AsReadOnly();

    /// <summary>
    /// Advances the spawner timer by <paramref name="deltaTime"/> seconds.
    /// Spawns a random enemy when the interval elapses and the cap is not reached.
    /// </summary>
    /// <param name="deltaTime">Elapsed time in seconds since the last update.</param>
    public void Update(float deltaTime)
    {
        _timeSinceLastSpawn += deltaTime;

        if (_timeSinceLastSpawn >= SpawnInterval && _enemies.Count < MaxEnemies)
        {
            SpawnRandom();
            _timeSinceLastSpawn = 0f;
        }
    }

    /// <summary>
    /// Spawns an enemy of a specific <paramref name="type"/> at the given position.
    /// </summary>
    public Enemy Spawn(EnemyType type, float x, float y)
    {
        var enemy = _factory.CreateEnemy(type, _nextId++, x, y);
        _enemies.Add(enemy);
        return enemy;
    }

    /// <summary>
    /// Spawns a randomly chosen enemy type at a random grid position.
    /// </summary>
    public Enemy SpawnRandom()
    {
        var types = Enum.GetValues<EnemyType>();
        var type = types[_random.Next(types.Length)];

        float x = _random.Next(0, GridSize);
        float y = _random.Next(0, GridSize);

        return Spawn(type, x, y);
    }

    /// <summary>
    /// Removes the enemy with the given <paramref name="id"/> from the active list.
    /// </summary>
    /// <returns><c>true</c> if the enemy was found and removed.</returns>
    public bool RemoveEnemy(int id)
    {
        var enemy = _enemies.FirstOrDefault(e => e.Id == id);
        return enemy is not null && _enemies.Remove(enemy);
    }

    /// <summary>
    /// Returns the current enemy list as shared <see cref="EnemyState"/> snapshots
    /// suitable for network transmission.
    /// </summary>
    public List<EnemyState> GetSnapshot() =>
        _enemies.Select(e => e.ToState()).ToList();
}
