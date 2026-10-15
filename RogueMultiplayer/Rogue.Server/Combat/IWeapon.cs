namespace Rogue.Server.Combat;

/// <summary>
/// Defines the contract for player weapons.
/// Designed for future Decorator pattern extensions (damage, attack speed, range modifiers).
/// </summary>
public interface IWeapon
{
    /// <summary>Damage dealt per successful hit.</summary>
    float Damage { get; }

    /// <summary>Interval in seconds between attacks.</summary>
    float Interval { get; }

    /// <summary>Reach of the attack along the facing direction, in grid units.</summary>
    float Reach { get; }

    /// <summary>Half-width of the attack perpendicular to the facing direction, in grid units.</summary>
    float HalfWidth { get; }
}
