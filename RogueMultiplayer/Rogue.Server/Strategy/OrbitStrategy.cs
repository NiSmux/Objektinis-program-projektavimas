using System.Numerics;
using Rogue.Server.Enemies;

namespace Rogue.Server.Strategy;

/// <summary>
/// Circles around the target while slowly spiralling inward until it
/// reaches a minimum radius, then keeps circling at that radius.
/// Even-id enemies orbit one way, odd-id enemies the other.
/// Stateless, so one instance may be shared.
/// </summary>
public class OrbitStrategy : IMovementStrategy
{
    private readonly float _minRadius;
    private readonly float _closingSpeed;

    /// <param name="minRadius">Closest orbit distance to the target, in tiles.</param>
    /// <param name="closingSpeed">Inward speed while spiralling, in tiles per second.</param>
    public OrbitStrategy(float minRadius = 1.5f, float closingSpeed = 0.75f)
    {
        _minRadius = minRadius;
        _closingSpeed = closingSpeed;
    }

    public Vector2 CalculateMove(Enemy self, Vector2 targetPosition, float deltaTime)
    {
        Vector2 toTarget = targetPosition - self.Position;
        float distance = toTarget.Length();

        Vector2 inward = distance > 0.001f ? toTarget / distance : Vector2.UnitX;
        float spin = self.Id % 2 == 0 ? 1f : -1f;
        Vector2 tangent = new Vector2(-inward.Y, inward.X) * spin;

        // Positive when too far (move in), negative when too close (move out).
        float radiusError = Math.Clamp(distance - _minRadius, -1f, 1f);

        Vector2 velocity = tangent * self.Speed + inward * (radiusError * _closingSpeed);

        return velocity * deltaTime;
    }
}
