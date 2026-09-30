using System.Numerics;
using Rogue.Server.Enemies;

namespace Rogue.Server.Strategy;

/// <summary>
/// Moves in a straight line toward the target at the enemy's speed.
/// Default strategy for every enemy. Stateless, so one instance may be shared.
/// </summary>
public class ChaseStrategy : IMovementStrategy
{
    public Vector2 CalculateMove(Enemy self, Vector2 targetPosition, float deltaTime)
    {
        Vector2 toTarget = targetPosition - self.Position;
        float distance = toTarget.Length();

        if (distance < 0.001f)
            return Vector2.Zero;

        // Never step past the target, otherwise the enemy jitters around it.
        float step = MathF.Min(self.Speed * deltaTime, distance);

        return toTarget / distance * step;
    }
}
