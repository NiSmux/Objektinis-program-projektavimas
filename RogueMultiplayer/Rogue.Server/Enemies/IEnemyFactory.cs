using Rogue.Shared.Models;

namespace Rogue.Server.Enemies;

/// <summary>
/// Defines the contract for the enemy factory.
/// </summary>
public interface IEnemyFactory
{
    /// <summary>
    /// Creates an enemy of the specified type at the given position.
    /// </summary>
    /// <param name="type">The type of enemy to create.</param>
    /// <param name="id">Unique identifier for the enemy.</param>
    /// <param name="x">Starting X position on the grid.</param>
    /// <param name="y">Starting Y position on the grid.</param>
    /// <returns>A new <see cref="Enemy"/> instance.</returns>
    Enemy CreateEnemy(EnemyType type, int id, float x, float y);
}
