using Rogue.Server.Strategy;
using Rogue.Shared.Models;

namespace Rogue.Server.Enemies;

/// <summary>
/// A slow but heavily armoured enemy with very high health and low speed.
/// Knows several movement strategies and cycles Chase → Dash → Orbit on a timer,
/// with exactly one of them active at a time.
/// </summary>
public class TankEnemy : Enemy
{
    private const float PhaseDuration = 4f;

    private readonly IMovementStrategy[] _phases;
    private int _phaseIndex;
    private float _phaseTimer;

    public override EnemyType EnemyType => EnemyType.Tank;

    public TankEnemy()
    {
        MaxHealth = 300f;
        Health = MaxHealth;
        Speed = 1.5f;

        // Own instances: DashStrategy keeps per-enemy timers.
        _phases = new IMovementStrategy[]
        {
            new ChaseStrategy(),
            new DashStrategy(),
            new OrbitStrategy()
        };

        SetMovementStrategy(_phases[0]);
    }

    /// <summary>The tank itself decides when to switch: every <see cref="PhaseDuration"/> seconds.</summary>
    protected override void ChooseStrategy(float deltaTime)
    {
        // Fleeing (low health) overrides the attack cycle.
        if (IsFleeing)
            return;

        _phaseTimer += deltaTime;
        if (_phaseTimer < PhaseDuration)
            return;

        _phaseTimer = 0f;
        _phaseIndex = (_phaseIndex + 1) % _phases.Length;
        SetMovementStrategy(_phases[_phaseIndex]);
    }
}
