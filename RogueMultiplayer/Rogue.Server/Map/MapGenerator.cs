namespace Rogue.Server.Map;

/// <summary>
/// Client of the abstract factory. It only knows <see cref="IMapThemeFactory"/>,
/// never the concrete water/lava classes.
/// </summary>
public class MapGenerator
{
    private readonly IMapThemeFactory _westTheme;
    private readonly IMapThemeFactory _eastTheme;
    private readonly Random _random = new();

    public double WallChance { get; set; } = 0.18;
    public double FloorChance { get; set; } = 0.12;

    public MapGenerator(IMapThemeFactory westTheme, IMapThemeFactory eastTheme)
    {
        _westTheme = westTheme;
        _eastTheme = eastTheme;
    }

    public List<Tile> Generate(int gridSize, int spawnX, int spawnY)
    {
        var tiles = new List<Tile>();

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                // Keep the player spawn area clear.
                if (Math.Abs(x - spawnX) <= 1 && Math.Abs(y - spawnY) <= 1)
                    continue;

                IMapThemeFactory theme = x < gridSize / 2 ? _westTheme : _eastTheme;
                double roll = _random.NextDouble();

                if (roll < WallChance)
                    tiles.Add(theme.CreateWall(x, y));
                else if (roll < WallChance + FloorChance)
                    tiles.Add(theme.CreateFloor(x, y));
            }
        }

        return tiles;
    }
}