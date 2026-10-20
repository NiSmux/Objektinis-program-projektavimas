namespace Rogue.Server.Combat;

/// <summary>
/// Concrete decorator that increases attack reach and half-width coverage
/// by adding flat bonuses to <see cref="IWeapon.Reach"/> and <see cref="IWeapon.HalfWidth"/>.
/// Enforces <see cref="MaxReach"/> and <see cref="MaxHalfWidth"/> caps.
/// </summary>
public class RangeDecorator : WeaponDecorator
{
    /// <summary>
    /// Maximum allowed attack reach in grid units.
    /// </summary>
    public const float MaxReach = 5.0f;

    /// <summary>
    /// Maximum allowed attack half-width in grid units.
    /// </summary>
    public const float MaxHalfWidth = 3.0f;

    /// <summary>
    /// Gets the flat bonus added to reach.
    /// </summary>
    public float ReachBonus { get; }

    /// <summary>
    /// Gets the flat bonus added to half-width.
    /// </summary>
    public float HalfWidthBonus { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeDecorator"/> class.
    /// </summary>
    /// <param name="inner">The inner weapon to decorate.</param>
    /// <param name="reachBonus">The flat bonus to add to reach. Defaults to 0.8.</param>
    /// <param name="halfWidthBonus">The flat bonus to add to half-width. Defaults to 0.5.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> is null.</exception>
    public RangeDecorator(IWeapon inner, float reachBonus = 0.8f, float halfWidthBonus = 0.5f)
        : base(inner)
    {
        ReachBonus = reachBonus;
        HalfWidthBonus = halfWidthBonus;
    }

    /// <inheritdoc />
    public override float Reach => MathF.Min(MaxReach, Inner.Reach + ReachBonus);

    /// <inheritdoc />
    public override float HalfWidth => MathF.Min(MaxHalfWidth, Inner.HalfWidth + HalfWidthBonus);
}
