namespace Rogue.Client.Subsystems;

using Rogue.Client.Entities;
using Rogue.Shared.Models;
using Rogue.Shared.Packets;

/// <summary>
/// Subsystem class responsible for tracking the local game world: players, enemies, tiles,
/// and synchronizing authoritatively with server state snapshots.
/// </summary>
public class WorldStateManager
{
    private bool _hasSyncedInitialWeapon;

    public Dictionary<int, Player> Players { get; } = new();
    public Dictionary<int, EnemyState> Enemies { get; } = new();
    public List<TileState> Tiles { get; private set; } = new();

    public Player LocalPlayer { get; } = new();
    public int MyId { get; private set; } = -1;

    /// <summary>
    /// Event triggered when the local player takes damage: (previousHp, newHp, damageAmount).
    /// </summary>
    public event Action<float, float, float>? LocalPlayerDamaged;

    /// <summary>
    /// Event triggered when the server confirms an authoritative weapon upgrade or modification.
    /// </summary>
    public event Action<Player>? LocalPlayerWeaponUpdated;

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
                AttackAnimTimer = state.AttackAnimTimer,
                WeaponDamage = state.WeaponDamage,
                WeaponInterval = state.WeaponInterval,
                WeaponReach = state.WeaponReach,
                WeaponHalfWidth = state.WeaponHalfWidth,
                WeaponName = state.WeaponName,
                BaseWeaponName = state.BaseWeaponName
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

            // Detect weapon upgrade confirmed by server
            bool weaponChanged = _hasSyncedInitialWeapon &&
                (LocalPlayer.WeaponDamage != authoritativePlayer.WeaponDamage ||
                 LocalPlayer.WeaponInterval != authoritativePlayer.WeaponInterval ||
                 LocalPlayer.WeaponReach != authoritativePlayer.WeaponReach ||
                 LocalPlayer.WeaponHalfWidth != authoritativePlayer.WeaponHalfWidth ||
                 LocalPlayer.WeaponName != authoritativePlayer.WeaponName);

            _hasSyncedInitialWeapon = true;

            LocalPlayer.X = authoritativePlayer.X;
            LocalPlayer.Y = authoritativePlayer.Y;
            LocalPlayer.Health = authoritativePlayer.Health;
            LocalPlayer.MaxHealth = authoritativePlayer.MaxHealth;
            LocalPlayer.AttackAnimTimer = authoritativePlayer.AttackAnimTimer;
            LocalPlayer.WeaponDamage = authoritativePlayer.WeaponDamage;
            LocalPlayer.WeaponInterval = authoritativePlayer.WeaponInterval;
            LocalPlayer.WeaponReach = authoritativePlayer.WeaponReach;
            LocalPlayer.WeaponHalfWidth = authoritativePlayer.WeaponHalfWidth;
            LocalPlayer.WeaponName = authoritativePlayer.WeaponName;
            LocalPlayer.BaseWeaponName = authoritativePlayer.BaseWeaponName;

            if (weaponChanged)
            {
                LocalPlayerWeaponUpdated?.Invoke(LocalPlayer);
            }
        }
    }
}
