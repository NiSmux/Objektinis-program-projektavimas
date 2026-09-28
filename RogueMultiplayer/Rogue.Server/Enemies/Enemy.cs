using Rogue.Shared.Models;

namespace Rogue.Server.Enemies;

/// <summary>
/// Abstract base class for all enemy types.
/// Concrete enemy classes inherit and set their own default stats.
/// </summary>
public abstract class Enemy
{
    public int Id { get; set; }

    public float X { get; set; }

    public float Y { get; set; }

    public float Health { get; set; }

    public float Speed { get; set; }

    public abstract EnemyType EnemyType { get; }

    /// <summary>Converts this server-side enemy to a shared EnemyState for network transmission.</summary>
    public EnemyState ToState() => new()
    {
        Id = Id,
        X = X,
        Y = Y,
        Type = EnemyType
    };
}
