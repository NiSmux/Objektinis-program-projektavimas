using Rogue.Shared.Models;

namespace Rogue.Server.Enemies;

/// <summary>
/// A slow but heavily armoured enemy with very high health and low speed.
/// </summary>
public class TankEnemy : Enemy
{
    public override EnemyType EnemyType => EnemyType.Tank;

    public TankEnemy()
    {
        Health = 300f;
        Speed = 1.5f;
    }
}
