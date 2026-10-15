using Rogue.Shared.Models;

namespace Rogue.Server.Map;

/// <summary>Cooled magma rock you can walk on.</summary>
public class LavaFloor : Floor
{
    public override TileType Type => TileType.LavaFloor;
}