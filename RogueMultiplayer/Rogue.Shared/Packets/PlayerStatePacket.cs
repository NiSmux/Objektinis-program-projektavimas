using Rogue.Shared.Models;

namespace Rogue.Shared.Packets;

public class GameStatePacket
{
    public int YourPlayerId { get; set; }

    public List<PlayerState> Players { get; set; } = new();

    public List<EnemyState> Enemies { get; set; } = new();
}