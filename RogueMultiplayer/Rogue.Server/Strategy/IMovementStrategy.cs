using System.Numerics;
using Rogue.Server.Enemies;

namespace Rogue.Server.Strategy;

/// <summary>
/// Strategy abstraction for enemy movement (GoF Strategy).
/// Each implementation is one interchangeable movement algorithm;
/// <see cref="Enemy"/> (the Context) holds one and delegates to it every tick.
/// </summary>
public interface IMovementStrategy
{
    /// <summary>
    /// Calculates how far the enemy should move during this tick.
    /// </summary>
    /// <param name="self">The enemy being moved (read-only use: position, speed, age, id).</param>
    /// <param name="targetPosition">Position of the player the enemy is reacting to, in tiles.</param>
    /// <param name="deltaTime">Elapsed time in seconds since the last tick.</param>
    /// <returns>Displacement (in tiles) to add to the enemy's position this tick.</returns>
    Vector2 CalculateMove(Enemy self, Vector2 targetPosition, float deltaTime);
}
