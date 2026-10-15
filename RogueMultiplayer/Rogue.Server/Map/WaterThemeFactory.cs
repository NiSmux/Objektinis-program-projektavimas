namespace Rogue.Server.Map;

public class WaterThemeFactory : IMapThemeFactory
{
    public string Name => "Water";

    public Wall CreateWall(int x, int y) => new WaterWall { X = x, Y = y };

    public Floor CreateFloor(int x, int y) => new WaterFloor { X = x, Y = y };
}