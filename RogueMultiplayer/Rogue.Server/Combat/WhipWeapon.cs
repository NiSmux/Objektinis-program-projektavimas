namespace Rogue.Server.Combat;

/// <summary>
/// Basic directional whip weapon inspired by Vampire Survivors.
/// Strikes in an oriented rectangular area directly in front of the player.
/// </summary>
public class WhipWeapon : IWeapon
{
    public string Name => "Whip";

    public float Damage { get; } = 25f;

    public float Interval { get; } = 1.0f;

    // Approximately 1 grid tile reach in front of the player
    public float Reach { get; } = 1.6f;

    // Perpendicular half-width 
    public float HalfWidth { get; } = 1f;
}
