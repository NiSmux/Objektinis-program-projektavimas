using Rogue.Server.Map;

namespace Rogue.Server.Builder;

/// <summary>
/// Concrete builder: a symmetric arena with a closed border,
/// four corner pillars and a floor cross in the middle.
/// Tiles are created by the theme factory (Abstract Factory).
/// </summary>
public class ArenaMapBuilder : IMapBuilder
{
    private readonly IMapThemeFactory _theme;
    private GameMap _map = new(0);

    public ArenaMapBuilder(IMapThemeFactory theme) => _theme = theme;

    public void Reset(int gridSize) => _map = new GameMap(gridSize);

    public void BuildOuterWalls()
    {
        int last = _map.GridSize - 1;
        for (int i = 0; i <= last; i++)
        {
            _map.TryAdd(_theme.CreateWall(i, 0));
            _map.TryAdd(_theme.CreateWall(i, last));
            _map.TryAdd(_theme.CreateWall(0, i));
            _map.TryAdd(_theme.CreateWall(last, i));
        }
    }

    public void BuildObstacles()
    {
        int near = 2;
        int far = _map.GridSize - 3;
        _map.TryAdd(_theme.CreateWall(near, near));
        _map.TryAdd(_theme.CreateWall(near, far));
        _map.TryAdd(_theme.CreateWall(far, near));
        _map.TryAdd(_theme.CreateWall(far, far));
    }

    public void BuildFloors()
    {
        int mid = _map.GridSize / 2;
        _map.TryAdd(_theme.CreateFloor(mid, mid));
        _map.TryAdd(_theme.CreateFloor(mid - 1, mid));
        _map.TryAdd(_theme.CreateFloor(mid + 1, mid));
        _map.TryAdd(_theme.CreateFloor(mid, mid - 1));
        _map.TryAdd(_theme.CreateFloor(mid, mid + 1));
    }

    public void ClearSpawn(int spawnX, int spawnY)
    {
        // Only the spawn cell itself, so the arena stays as symmetric as possible.
        _map.Remove(spawnX, spawnY);
    }

    public GameMap GetResult() => _map;
}
