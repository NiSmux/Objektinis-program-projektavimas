namespace Rogue.Server.Combat;

/// <summary>
/// Concrete decorator that decreases attack interval by dividing <see cref="IWeapon.Interval"/>
/// by a speed multiplier. Enforces a <see cref="MinInterval"/> to prevent geometric interval explosion.
/// </summary>
public class AttackSpeedDecorator : WeaponDecorator
{
    /// <summary>
    /// Absolute minimum allowed attack interval (in seconds) to avoid tick saturation.
    /// </summary>
    public const float MinInterval = 0.15f;

    /// <summary>
    /// Gets the speed multiplier applied to the attack interval.
    /// </summary>
    public float SpeedMultiplier { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttackSpeedDecorator"/> class.
    /// </summary>
    /// <param name="inner">The inner weapon to decorate.</param>
    /// <param name="speedMultiplier">
    /// The multiplier by which the interval is divided (must be greater than 0). Defaults to 1.5.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="speedMultiplier"/> is less than or equal to 0.</exception>
    public AttackSpeedDecorator(IWeapon inner, float speedMultiplier = 1.5f)
        : base(inner)
    {
        if (speedMultiplier <= 0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(speedMultiplier),
                speedMultiplier,
                "Speed multiplier must be greater than zero.");
        }

        SpeedMultiplier = speedMultiplier;
    }

    /// <inheritdoc />
    public override float Interval => MathF.Max(MinInterval, Inner.Interval / SpeedMultiplier);
}
