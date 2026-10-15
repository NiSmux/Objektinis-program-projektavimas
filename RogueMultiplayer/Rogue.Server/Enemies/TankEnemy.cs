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

    // Not readonly: DeepCopy replaces it with cloned phases.
    private IMovementStrategy[] _phases;
    private int _phaseIndex;
    private float _phaseTimer;

    public override EnemyType EnemyType => EnemyType.Tank;

    public TankEnemy()
    {
        MaxHealth = 300f;
        Health = MaxHealth;
        Speed = 1.3f;

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
        // Enraged (low health) overrides the attack cycle.
        if (IsEnraged)
            return;

        _phaseTimer += deltaTime;
        if (_phaseTimer < PhaseDuration)
            return;

        _phaseTimer = 0f;
        _phaseIndex = (_phaseIndex + 1) % _phases.Length;
        SetMovementStrategy(_phases[_phaseIndex]);
    }

    /// <summary>
    /// Deep copy also clones the phase array, and keeps the active strategy
    /// pointing at the copy's own phase instead of the original's.
    /// </summary>
    protected override void CloneReferences()
    {
        int activePhase = Array.IndexOf(_phases, GetMovementStrategy());

        _phases = _phases.Select(phase => phase.Clone()).ToArray();

        if (activePhase >= 0)
            SetMovementStrategy(_phases[activePhase]);
        else
            base.CloneReferences(); // enraged: active strategy is not one of the phases
    }
}
