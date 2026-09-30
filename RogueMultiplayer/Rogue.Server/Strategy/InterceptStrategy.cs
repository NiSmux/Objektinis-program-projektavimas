using System.Numerics;
using Rogue.Server.Enemies;

namespace Rogue.Server.Strategy;

/// <summary>
/// Predicts where the target is heading and runs to cut it off, instead of
/// running at where the target is now. The target's velocity is estimated
/// from its position on the previous tick.
/// Holds per-enemy state (last seen target position), so each enemy needs its own instance.
/// </summary>
public class InterceptStrategy : IMovementStrategy
{
    private readonly float _maxLookAhead;

    private bool _hasLastTarget;
    private Vector2 _lastTargetPosition;

    /// <param name="maxLookAhead">Maximum seconds into the future to predict the target's position.</param>
    public InterceptStrategy(float maxLookAhead = 1.5f)
    {
        _maxLookAhead = maxLookAhead;
    }

    public Vector2 CalculateMove(Enemy self, Vector2 targetPosition, float deltaTime)
    {
        Vector2 targetVelocity = _hasLastTarget && deltaTime > 0f
            ? (targetPosition - _lastTargetPosition) / deltaTime
            : Vector2.Zero;

        _lastTargetPosition = targetPosition;
        _hasLastTarget = true;

        // Look further ahead the further away the target is (roughly our travel time).
        float distance = Vector2.Distance(self.Position, targetPosition);
        float lookAhead = MathF.Min(distance / self.Speed, _maxLookAhead);
        Vector2 aimPoint = targetPosition + targetVelocity * lookAhead;

        Vector2 toAim = aimPoint - self.Position;
        float aimDistance = toAim.Length();

        if (aimDistance < 0.001f)
            return Vector2.Zero;

        float step = MathF.Min(self.Speed * deltaTime, aimDistance);

        return toAim / aimDistance * step;
    }
}
