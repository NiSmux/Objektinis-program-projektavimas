using System.Numerics;
using Rogue.Server.Game;
using Rogue.Server.Strategy;
using Rogue.Shared.Models;

namespace Rogue.Server.Enemies;

/// <summary>
/// Abstract base class for all enemy types.
/// Concrete enemy classes inherit and set their own default stats.
/// Acts as the Context of the Strategy pattern: movement is delegated to an
/// <see cref="IMovementStrategy"/> that can be swapped at runtime.
/// </summary>
public abstract class Enemy
{
    /// <summary>Health fraction below which the enemy becomes enraged.</summary>
    private const float EnrageHealthFraction = 0.25f;

    private IMovementStrategy _movementStrategy;

    protected Enemy()
    {
        // Default strategy, like Unit → Drive in the lecture example.
        _movementStrategy = new ChaseStrategy();
    }

    public int Id { get; set; }

    public float X { get; set; }

    public float Y { get; set; }

    public float Health { get; set; }

    public float MaxHealth { get; protected set; }

    public float Speed { get; set; }

    /// <summary>Seconds this enemy has been updated for (used by time-based strategies).</summary>
    public float Age { get; private set; }

    /// <summary>True once low health has made this enemy switch to intercepting.</summary>
    public bool IsEnraged { get; private set; }

    public Vector2 Position => new(X, Y);

    public abstract EnemyType EnemyType { get; }

    public IMovementStrategy GetMovementStrategy() => _movementStrategy;

    public void SetMovementStrategy(IMovementStrategy strategy)
    {
        _movementStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }

    /// <summary>
    /// Advances the enemy by one tick: lets the enemy pick its strategy,
    /// then delegates the actual movement to the current strategy.
    /// </summary>
    /// <param name="targetPosition">Position of the player this enemy reacts to.</param>
    /// <param name="deltaTime">Elapsed time in seconds since the last tick.</param>
    public void Update(Vector2 targetPosition, float deltaTime)
    {
        Age += deltaTime;

        ChooseStrategy(deltaTime);

        Vector2 move = _movementStrategy.CalculateMove(this, targetPosition, deltaTime);

        int maxCoord = GameSettings.Instance.GridSize - 1;
        X = Math.Clamp(X + move.X, 0, maxCoord);
        Y = Math.Clamp(Y + move.Y, 0, maxCoord);
    }

    /// <summary>
    /// Reduces health. When health drops below 25% the enemy itself decides
    /// to get enraged and switch to <see cref="InterceptStrategy"/>.
    /// </summary>
    public void TakeDamage(float amount)
    {
        Health = MathF.Max(0f, Health - amount);

        if (!IsEnraged && Health < MaxHealth * EnrageHealthFraction)
        {
            IsEnraged = true;
            // New instance: InterceptStrategy remembers this enemy's last target position.
            SetMovementStrategy(new InterceptStrategy());
        }
    }

    /// <summary>
    /// Hook called every tick before moving. Subclasses override it to swap
    /// their own strategy (e.g. on a timer). Does nothing by default.
    /// </summary>
    protected virtual void ChooseStrategy(float deltaTime) { }

    /// <summary>Converts this server-side enemy to a shared EnemyState for network transmission.</summary>
    public EnemyState ToState() => new()
    {
        Id = Id,
        X = X,
        Y = Y,
        Type = EnemyType
    };
}
