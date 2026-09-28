using Rogue.Shared.Models;

namespace Rogue.Server.Enemies;

/// <summary>
/// A fast but fragile enemy with low health and high speed.
/// </summary>
public class FastEnemy : Enemy
{
    public override EnemyType EnemyType => EnemyType.Fast;

    public FastEnemy()
    {
        Health = 50f;
        Speed = 7f;
    }
}
