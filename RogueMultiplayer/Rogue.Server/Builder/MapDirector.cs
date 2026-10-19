namespace Rogue.Server.Builder;

/// <summary>
/// Director of the Builder pattern: knows the ORDER of the build steps,
/// but not how any builder implements them.
/// </summary>
public class MapDirector
{
    /// <summary>Full map: border, obstacles, floors, then a clear spawn.</summary>
    public GameMap BuildFullMap(IMapBuilder builder, int gridSize, int spawnX, int spawnY)
    {
        builder.Reset(gridSize);
        builder.BuildOuterWalls();
        builder.BuildObstacles();
        builder.BuildFloors();
        builder.ClearSpawn(spawnX, spawnY);
        return builder.GetResult();
    }

    /// <summary>Open map: same as the full map, but without the outer walls.</summary>
    public GameMap BuildOpenMap(IMapBuilder builder, int gridSize, int spawnX, int spawnY)
    {
        builder.Reset(gridSize);
        builder.BuildObstacles();
        builder.BuildFloors();
        builder.ClearSpawn(spawnX, spawnY);
        return builder.GetResult();
    }
}
