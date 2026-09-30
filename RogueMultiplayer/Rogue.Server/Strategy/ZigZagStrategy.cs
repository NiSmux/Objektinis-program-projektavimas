using System.Numerics;
using Rogue.Server.Enemies;

namespace Rogue.Server.Strategy;

/// <summary>
/// Moves toward the target while weaving side to side along a sine wave.
/// The wave phase comes from the enemy's own age and id, so the strategy
/// itself holds no per-enemy state and one instance may be shared.
/// </summary>
public class ZigZagStrategy : IMovementStrategy
{
    private readonly float _amplitude;
    private readonly float _frequency;

    /// <param name="amplitude">Sideways speed as a fraction of forward speed.</param>
    /// <param name="frequency">Weave frequency in radians per second.</param>
    public ZigZagStrategy(float amplitude = 1.2f, float frequency = 6f)
    {
        _amplitude = amplitude;
        _frequency = frequency;
    }

    public Vector2 CalculateMove(Enemy self, Vector2 targetPosition, float deltaTime)
    {
        Vector2 toTarget = targetPosition - self.Position;
        float distance = toTarget.Length();
        float forwardStep = self.Speed * deltaTime;

        // Close enough: finish the approach without weaving past the target.
        if (distance <= forwardStep)
            return toTarget;

        Vector2 forward = toTarget / distance;
        Vector2 side = new(-forward.Y, forward.X); // perpendicular to forward

        // Offset each enemy's wave by its id so a group doesn't weave in sync.
        float wave = MathF.Cos(self.Age * _frequency + self.Id);

        return (forward + side * (_amplitude * wave)) * forwardStep;
    }
}
