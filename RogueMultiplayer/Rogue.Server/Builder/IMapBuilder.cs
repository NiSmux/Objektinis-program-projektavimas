namespace Rogue.Server.Builder;

/// <summary>
/// Builder interface: the steps needed to assemble a <see cref="GameMap"/>.
/// Each concrete builder implements the steps in its own way.
/// </summary>
public interface IMapBuilder
{
    void Reset(int gridSize);

    void BuildOuterWalls();

    void BuildObstacles();

    void BuildFloors();

    void ClearSpawn(int spawnX, int spawnY);

    GameMap GetResult();
}
