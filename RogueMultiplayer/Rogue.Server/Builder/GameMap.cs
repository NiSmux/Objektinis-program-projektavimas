using System.Text;
using Rogue.Server.Map;
using Rogue.Shared.Models;

namespace Rogue.Server.Builder;

/// <summary>
/// Product of the Builder pattern: a finished map made of tiles.
/// At most one tile per grid cell; empty cells have no tile.
/// </summary>
public class GameMap
{
    private readonly List<Tile> _tiles = new();

    public GameMap(int gridSize) => GridSize = gridSize;

    public int GridSize { get; }

    public IReadOnlyList<Tile> Tiles => _tiles;

    public Tile? GetTile(int x, int y) => _tiles.FirstOrDefault(t => t.X == x && t.Y == y);

    /// <summary>Adds the tile only if its cell is inside the map and still empty.</summary>
    public bool TryAdd(Tile tile)
    {
        if (tile.X < 0 || tile.Y < 0 || tile.X >= GridSize || tile.Y >= GridSize)
            return false;
        if (GetTile(tile.X, tile.Y) != null)
            return false;

        _tiles.Add(tile);
        return true;
    }

    public void Remove(int x, int y) => _tiles.RemoveAll(t => t.X == x && t.Y == y);

    /// <summary>Text picture of the map: # wall, ~ floor, . empty.</summary>
    public string ToAscii()
    {
        var sb = new StringBuilder();
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                sb.Append(GetTile(x, y)?.Type switch
                {
                    TileType.WaterWall or TileType.LavaWall => '#',
                    TileType.WaterFloor or TileType.LavaFloor => '~',
                    _ => '.'
                });
                sb.Append(' ');
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
