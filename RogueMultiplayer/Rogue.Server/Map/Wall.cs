namespace Rogue.Server.Map;

/// <summary>Abstract product A: a solid tile that blocks movement.</summary>
public abstract class Wall : Tile
{
    public override bool IsBlocking => true;
}