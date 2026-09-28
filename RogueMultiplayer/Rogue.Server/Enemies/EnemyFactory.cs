using Rogue.Shared.Models;

namespace Rogue.Server.Enemies;

/// <summary>
/// Concrete implementation of <see cref="IEnemyFactory"/>.
/// Instantiates the correct enemy subclass based on the requested <see cref="EnemyType"/>.
/// </summary>
public class EnemyFactory : IEnemyFactory
{
    public Enemy CreateEnemy(EnemyType type, int id, float x, float y)
    {
        Enemy enemy = type switch
        {
            EnemyType.Basic => new BasicEnemy(),
            EnemyType.Fast  => new FastEnemy(),
            EnemyType.Tank  => new TankEnemy(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown enemy type.")
        };

        enemy.Id = id;
        enemy.X  = x;
        enemy.Y  = y;

        return enemy;
    }
}
