namespace Rogue.Shared.Packets;

/// <summary>
/// Network packet sent from client to server requesting a specific weapon decorator upgrade.
/// </summary>
public class UpgradePacket
{
    /// <summary>
    /// Type of upgrade requested: "damage", "speed", "range", or "reset".
    /// </summary>
    public string UpgradeType { get; set; } = string.Empty;
}

