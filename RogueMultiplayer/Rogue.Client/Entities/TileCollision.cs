using Rogue.Shared.Models;

namespace Rogue.Client.Entities;

public static class TileCollision
{
    /// <summary>True if a player at (x, y) would overlap a wall tile.</summary>
    public static bool IsBlocked(IEnumerable<TileState> tiles, float x, float y)
    {
        const float e = 0.1f; // small tolerance so you don't snag on edges

        foreach (var t in tiles)
        {
            if (t.Type != TileType.WaterWall && t.Type != TileType.LavaWall)
                continue;

            bool overlapX = x + 1 - e > t.X && x + e < t.X + 1;
            bool overlapY = y + 1 - e > t.Y && y + e < t.Y + 1;

            if (overlapX && overlapY)
                return true;
        }

        return false;
    }
}