using Rogue.Shared.Models;

namespace Rogue.Server.Map;

/// <summary>Base class for every map tile (abstract product).</summary>
public abstract class Tile
{
    public int X { get; set; }

    public int Y { get; set; }

    public abstract TileType Type { get; }

    public abstract bool IsBlocking { get; }

    public TileState ToState() => new()
    {
        X = X,
        Y = Y,
        Type = Type
    };
}