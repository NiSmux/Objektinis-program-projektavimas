using Rogue.Shared.Models;

namespace Rogue.Server.Enemies;

/// <summary>
/// Prototype registry: keeps one ready-made prototype per <see cref="EnemyType"/>
/// and produces new enemies by deep-copying it instead of constructing them.
/// Implements <see cref="IEnemyFactory"/>, so <see cref="EnemySpawner"/> can use it
/// without knowing that enemies are cloned.
/// </summary>
public class EnemyPrototypeRegistry : IEnemyFactory
{
    private readonly Dictionary<EnemyType, Enemy> _prototypes = new();

    /// <summary>Builds the initial prototypes once, using <paramref name="factory"/>.</summary>
    public EnemyPrototypeRegistry(IEnemyFactory factory)
    {
        foreach (var type in Enum.GetValues<EnemyType>())
            Register(type, factory.CreateEnemy(type, id: 0, x: 0, y: 0));
    }

    /// <summary>Adds or replaces the prototype used for <paramref name="type"/>.</summary>
    public void Register(EnemyType type, Enemy prototype) => _prototypes[type] = prototype;

    public Enemy GetPrototype(EnemyType type) => _prototypes[type];

    public Enemy CreateEnemy(EnemyType type, int id, float x, float y)
    {
        if (!_prototypes.TryGetValue(type, out var prototype))
            throw new ArgumentOutOfRangeException(nameof(type), type, "No prototype registered.");

        // Deep copy: each spawned enemy must have its own strategy state (e.g. dash timers).
        Enemy enemy = prototype.DeepCopy();

        enemy.Id = id;
        enemy.X  = x;
        enemy.Y  = y;

        return enemy;
    }
}
