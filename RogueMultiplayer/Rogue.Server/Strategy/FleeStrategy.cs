using System.Numerics;
using Rogue.Server.Enemies;

namespace Rogue.Server.Strategy;

/// <summary>
/// Runs directly away from the target. Used by enemies whose health is low.
/// Stateless, so one instance is shared by all enemies (see <see cref="Enemy"/>).
/// </summary>
public class FleeStrategy : IMovementStrategy
{
    public Vector2 CalculateMove(Enemy self, Vector2 targetPosition, float deltaTime)
    {
        Vector2 away = self.Position - targetPosition;

        // Standing exactly on the target: pick any direction to escape.
        Vector2 direction = away.LengthSquared() > 0f ? Vector2.Normalize(away) : Vector2.UnitX;

        return direction * (self.Speed * deltaTime);
    }
}
