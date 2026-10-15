namespace Rogue.Server.Map;

/// <summary>Abstract product B: a walkable decorative tile.</summary>
public abstract class Floor : Tile
{
    public override bool IsBlocking => false;
}