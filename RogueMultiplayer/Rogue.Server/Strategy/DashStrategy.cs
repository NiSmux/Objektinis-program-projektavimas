using System.Numerics;
using Rogue.Server.Enemies;

namespace Rogue.Server.Strategy;

/// <summary>
/// Stands still to wind up, locks onto the target's current position, then
/// charges in that fixed direction at high speed (and can overshoot if the
/// player dodges). Repeats wind-up → charge.
/// Holds per-enemy timers, so each enemy needs its own instance.
/// </summary>
public class DashStrategy : IMovementStrategy
{
    private readonly float _windUpTime;
    private readonly float _dashTime;
    private readonly float _speedMultiplier;

    private bool _isDashing;
    private float _phaseTimer;
    private Vector2 _dashDirection;

    /// <param name="windUpTime">Seconds spent standing still before each charge.</param>
    /// <param name="dashTime">Seconds each charge lasts.</param>
    /// <param name="speedMultiplier">Charge speed as a multiple of the enemy's speed.</param>
    public DashStrategy(float windUpTime = 1f, float dashTime = 0.4f, float speedMultiplier = 3f)
    {
        _windUpTime = windUpTime;
        _dashTime = dashTime;
        _speedMultiplier = speedMultiplier;
        _phaseTimer = windUpTime;
    }

    public Vector2 CalculateMove(Enemy self, Vector2 targetPosition, float deltaTime)
    {
        _phaseTimer -= deltaTime;

        if (!_isDashing)
        {
            if (_phaseTimer > 0f)
                return Vector2.Zero; // winding up

            // Wind-up finished: aim once, then commit to that direction.
            Vector2 toTarget = targetPosition - self.Position;
            _dashDirection = toTarget.LengthSquared() > 0f ? Vector2.Normalize(toTarget) : Vector2.Zero;
            _isDashing = true;
            _phaseTimer = _dashTime;
        }
        else if (_phaseTimer <= 0f)
        {
            _isDashing = false;
            _phaseTimer = _windUpTime;
            return Vector2.Zero;
        }

        return _dashDirection * (self.Speed * _speedMultiplier * deltaTime);
    }
}
