namespace Rogue.Server.Game;

/// <summary>
/// Central, read-only source of server-wide tuning values (Singleton).
/// Replaces values that used to be duplicated/scattered across the server.
/// </summary>
public sealed class GameSettings
{
    private static readonly GameSettings instance = new();

    public static GameSettings Instance => instance;

    private GameSettings() { }

    /// <summary>Grid size used when randomising enemy spawn positions.</summary>
    public int GridSize { get; } = 9;

    /// <summary>Seconds between automatic enemy spawns.</summary>
    public float SpawnInterval { get; } = 5f;

    /// <summary>Maximum number of enemies allowed on the map at once.</summary>
    public int MaxEnemies { get; } = 20;

    /// <summary>Server game loop tick rate, in seconds per tick.</summary>
    public float TickRate { get; } = 1f / 20f;
}
