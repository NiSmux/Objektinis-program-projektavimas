using Rogue.Server.Map;

namespace Rogue.Server.Builder;

/// <summary>
/// Concrete builder: an irregular cave with a broken border,
/// randomly scattered rocks and puddles.
/// Tiles are created by the theme factory (Abstract Factory).
/// </summary>
public class CaveMapBuilder : IMapBuilder
{
    private const double BorderChance = 0.7;
    private const double RockChance = 0.2;
    private const double PuddleChance = 0.15;

    private readonly IMapThemeFactory _theme;
    private readonly Random _random;
    private GameMap _map = new(0);

    /// <param name="seed">Same seed gives the same cave.</param>
    public CaveMapBuilder(IMapThemeFactory theme, int seed)
    {
        _theme = theme;
        _random = new Random(seed);
    }

    public void Reset(int gridSize) => _map = new GameMap(gridSize);

    public void BuildOuterWalls()
    {
        int last = _map.GridSize - 1;
        for (int i = 0; i <= last; i++)
        {
            if (_random.NextDouble() < BorderChance) _map.TryAdd(_theme.CreateWall(i, 0));
            if (_random.NextDouble() < BorderChance) _map.TryAdd(_theme.CreateWall(i, last));
            if (_random.NextDouble() < BorderChance) _map.TryAdd(_theme.CreateWall(0, i));
            if (_random.NextDouble() < BorderChance) _map.TryAdd(_theme.CreateWall(last, i));
        }
    }

    public void BuildObstacles()
    {
        for (int x = 1; x < _map.GridSize - 1; x++)
            for (int y = 1; y < _map.GridSize - 1; y++)
                if (_random.NextDouble() < RockChance)
                    _map.TryAdd(_theme.CreateWall(x, y));
    }

    public void BuildFloors()
    {
        // TryAdd skips cells already taken by rocks.
        for (int x = 0; x < _map.GridSize; x++)
            for (int y = 0; y < _map.GridSize; y++)
                if (_random.NextDouble() < PuddleChance)
                    _map.TryAdd(_theme.CreateFloor(x, y));
    }

    public void ClearSpawn(int spawnX, int spawnY)
    {
        // Clear a 3x3 area so the player is never boxed in.
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                _map.Remove(spawnX + dx, spawnY + dy);
    }

    public GameMap GetResult() => _map;
}
