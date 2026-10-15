using Rogue.Shared.Models;

namespace Rogue.Server.Map;

public class WaterWall : Wall
{
    public override TileType Type => TileType.WaterWall;
}