using Rogue.Server.Strategy;
using Rogue.Shared.Models;

namespace Rogue.Server.Enemies;

/// <summary>
/// A fast but fragile enemy with low health and high speed.
/// Weaves toward the player instead of running straight at them.
/// </summary>
public class FastEnemy : Enemy
{
    public override EnemyType EnemyType => EnemyType.Fast;

    public FastEnemy()
    {
        MaxHealth = 20f;
        Health = MaxHealth;
        Speed = 2.9f;

        SetMovementStrategy(new ZigZagStrategy());
    }
}
