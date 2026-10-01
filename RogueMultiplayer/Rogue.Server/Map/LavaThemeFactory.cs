namespace Rogue.Server.Map;

public class LavaThemeFactory : IMapThemeFactory
{
    public string Name => "Lava";

    public Wall CreateWall(int x, int y) => new LavaWall { X = x, Y = y };

    public Floor CreateFloor(int x, int y) => new LavaFloor { X = x, Y = y };
}