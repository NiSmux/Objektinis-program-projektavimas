using Rogue.Shared.Models;

namespace Rogue.Server.Enemies;

/// <summary>
/// A standard enemy with balanced health and speed.
/// </summary>
public class BasicEnemy : Enemy
{
    public override EnemyType EnemyType => EnemyType.Basic;

    public BasicEnemy()
    {
        Health = 100f;
        Speed = 3f;
    }
}
