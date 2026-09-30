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
        MaxHealth = 100f;
        Health = MaxHealth;
        Speed = 3f;
    }
}
