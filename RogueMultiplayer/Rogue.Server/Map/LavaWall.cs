using Rogue.Shared.Models;

namespace Rogue.Server.Map;

public class LavaWall : Wall
{
    public override TileType Type => TileType.LavaWall;
}