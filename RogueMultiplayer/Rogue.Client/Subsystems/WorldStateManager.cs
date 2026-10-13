using Rogue.Client.Entities;
using Rogue.Shared.Models;
using Rogue.Shared.Packets;

namespace Rogue.Client.Subsystems;

/// <summary>
/// Subsystem class responsible for tracking the local game world: players, enemies, tiles,
/// and synchronizing authoritatively with server state snapshots.
/// </summary>
public class WorldStateManager
{
    public Dictionary<int, Player> Players { get; } = new();
    public Dictionary<int, EnemyState> Enemies { get; } = new();
    public List<TileState> Tiles { get; private set; } = new();

    public Player LocalPlayer { get; } = new();
    public int MyId { get; private set; } = -1;

    /// <summary>
    /// Event triggered when the local player takes damage: (previousHp, newHp, damageAmount).
    /// </summary>
    public event Action<float, float, float>? LocalPlayerDamaged;

    public void SyncFromPacket(GameStatePacket packet)
    {
        Tiles = packet.Tiles;

        Players.Clear();
        Enemies.Clear();

        foreach (var state in packet.Players)
        {
            Players[state.Id] = new Player
            {
                X = state.X,
                Y = state.Y,
                Health = state.Health,
                MaxHealth = state.MaxHealth,
                FacingDx = state.FacingDx,
                FacingDy = state.FacingDy,
                AttackAnimTimer = state.AttackAnimTimer
            };
        }

        foreach (var enemy in packet.Enemies)
        {
            Enemies[enemy.Id] = enemy;
        }

        MyId = packet.YourPlayerId;

        if (Players.TryGetValue(MyId, out Player? authoritativePlayer))
        {
            // Detect damage taken between snapshots
            if (LocalPlayer.Health > authoritativePlayer.Health)
            {
                float damageTaken = LocalPlayer.Health - authoritativePlayer.Health;
                LocalPlayerDamaged?.Invoke(LocalPlayer.Health, authoritativePlayer.Health, damageTaken);
            }

            LocalPlayer.X = authoritativePlayer.X;
            LocalPlayer.Y = authoritativePlayer.Y;
            LocalPlayer.Health = authoritativePlayer.Health;
            LocalPlayer.MaxHealth = authoritativePlayer.MaxHealth;
            LocalPlayer.AttackAnimTimer = authoritativePlayer.AttackAnimTimer;
        }
    }
}
