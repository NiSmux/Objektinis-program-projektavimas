using Rogue.Shared.Models;

namespace Rogue.Server.Map;

/// <summary>Shallow water you can walk through.</summary>
public class WaterFloor : Floor
{
    public override TileType Type => TileType.WaterFloor;
}