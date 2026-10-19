using Rogue.Server.Game;
using Rogue.Server.Map;

namespace Rogue.Server.Builder;

/// <summary>
/// Console demo of the Builder pattern: the same director builds maps with
/// two different concrete builders and two different step sequences.
/// Run with: dotnet run --project Rogue.Server -- --builder-demo
/// </summary>
public static class BuilderDemo
{
    private const int SpawnX = 3;
    private const int SpawnY = 3;

    public static void Run()
    {
        int gridSize = GameSettings.Instance.GridSize;
        var director = new MapDirector();

        Print("ArenaMapBuilder (water), full map",
              director.BuildFullMap(new ArenaMapBuilder(new WaterThemeFactory()), gridSize, SpawnX, SpawnY));

        Print("CaveMapBuilder (lava), full map",
              director.BuildFullMap(new CaveMapBuilder(new LavaThemeFactory(), seed: 42), gridSize, SpawnX, SpawnY));

        Print("ArenaMapBuilder (water), open map - director skips BuildOuterWalls",
              director.BuildOpenMap(new ArenaMapBuilder(new WaterThemeFactory()), gridSize, SpawnX, SpawnY));

        Print("CaveMapBuilder (lava), open map - director skips BuildOuterWalls",
              director.BuildOpenMap(new CaveMapBuilder(new LavaThemeFactory(), seed: 42), gridSize, SpawnX, SpawnY));
    }

    private static void Print(string title, GameMap map)
    {
        Console.WriteLine($"=== {title} ===");
        Console.WriteLine($"{map.Tiles.Count} tiles, first tile type: {map.Tiles.FirstOrDefault()?.Type}\n");
        Console.WriteLine(map.ToAscii());
    }
}
