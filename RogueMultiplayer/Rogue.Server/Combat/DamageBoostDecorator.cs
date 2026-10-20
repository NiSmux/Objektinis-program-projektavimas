namespace Rogue.Server.Combat;

/// <summary>
/// Concrete decorator that adds a flat bonus to the weapon's <see cref="IWeapon.Damage"/>.
/// Enforces <see cref="MaxDamage"/> cap.
/// </summary>
public class DamageBoostDecorator : WeaponDecorator
{
    /// <summary>
    /// Maximum allowed weapon damage cap.
    /// </summary>
    public const float MaxDamage = 250f;

    /// <summary>
    /// Gets the flat bonus added to damage.
    /// </summary>
    public float BonusDamage { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DamageBoostDecorator"/> class.
    /// </summary>
    /// <param name="inner">The inner weapon to decorate.</param>
    /// <param name="bonusDamage">The flat damage bonus to add. Defaults to 10.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> is null.</exception>
    public DamageBoostDecorator(IWeapon inner, float bonusDamage = 10f)
        : base(inner)
    {
        BonusDamage = bonusDamage;
    }

    /// <inheritdoc />
    public override float Damage => MathF.Min(MaxDamage, Inner.Damage + BonusDamage);
}
