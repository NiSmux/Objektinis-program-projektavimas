namespace Rogue.Server.Combat;

/// <summary>
/// Abstract base decorator for <see cref="IWeapon"/> implementations.
/// Holds a reference to the wrapped <see cref="Inner"/> weapon and delegates
/// all property calls to it as virtual members, allowing concrete decorators
/// to override only the properties they modify.
/// </summary>
public abstract class WeaponDecorator : IWeapon
{
    /// <summary>
    /// The wrapped inner weapon instance.
    /// </summary>
    protected readonly IWeapon Inner;

    /// <summary>
    /// Exposes the wrapped inner weapon instance for decorator chain inspection.
    /// </summary>
    public IWeapon InnerWeapon => Inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaponDecorator"/> class.
    /// </summary>
    /// <param name="inner">The inner weapon to decorate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> is null.</exception>
    protected WeaponDecorator(IWeapon inner)
    {
        Inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <inheritdoc />
    public virtual string Name => Inner.Name;

    /// <inheritdoc />
    public virtual float Damage => Inner.Damage;

    /// <inheritdoc />
    public virtual float Interval => Inner.Interval;

    /// <inheritdoc />
    public virtual float Reach => Inner.Reach;

    /// <inheritdoc />
    public virtual float HalfWidth => Inner.HalfWidth;

    /// <summary>
    /// Returns the names of all decorators applied in this chain, from innermost to outermost.
    /// </summary>
    public List<string> GetAppliedDecorators()
    {
        var decorators = new List<string>();
        IWeapon? current = this;
        while (current is WeaponDecorator decorator)
        {
            string shortName = decorator.GetType().Name.Replace("Decorator", "");
            decorators.Add(shortName);
            current = decorator.InnerWeapon;
        }
        decorators.Reverse();
        return decorators;
    }
}
