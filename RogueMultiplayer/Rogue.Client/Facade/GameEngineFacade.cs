using System.Drawing;
using Rogue.Client.Commands;
using Rogue.Client.Entities;
using Rogue.Client.Networking;
using Rogue.Client.Rendering;
using Rogue.Client.Subsystems;
using Rogue.Shared.Models;
using Rogue.Shared.Packets;

namespace Rogue.Client.Facade;

/// <summary>
/// Facade implementation coordinating five client subsystems:
/// 1. <see cref="NetworkClient"/> (Networking)
/// 2. <see cref="Renderer"/> (Graphics & HUD)
/// 3. <see cref="CommandInvoker"/> (Command execution & undo history)
/// 4. <see cref="WorldStateManager"/> (World state & entity tracking)
/// 5. <see cref="CombatFeedbackManager"/> (Floating combat text & announcements)
/// </summary>
public sealed class GameEngineFacade : IGameEngineFacade
{
    // ── Coordinated Subsystems ──────────────────────────────────────────────────
    private readonly NetworkClient _network;
    private readonly Renderer _renderer;
    private readonly CommandInvoker _invoker;
    private readonly WorldStateManager _world;
    private readonly CombatFeedbackManager _combatFeedback;

    // ── Public Facade Properties ────────────────────────────────────────────────
    public string ConnectionStatus { get; private set; } = "Disconnected";
    public bool IsConnected { get; private set; }

    public Player LocalPlayer => _world.LocalPlayer;
    public int LocalPlayerId => _world.MyId;
    public int EnemyCount => _world.Enemies.Count;
    public int PlayerCount => _world.Players.Count;
    public IReadOnlyList<TileState> Tiles => _world.Tiles;

    // ── Facade Events ───────────────────────────────────────────────────────────
    public event Action? StateChanged;
    public event Action<string, Color>? NotificationReceived;

    public GameEngineFacade(
        NetworkClient? network = null,
        Renderer? renderer = null,
        CommandInvoker? invoker = null,
        WorldStateManager? world = null,
        CombatFeedbackManager? combatFeedback = null)
    {
        _network = network ?? new NetworkClient();
        _renderer = renderer ?? new Renderer();
        _invoker = invoker ?? new CommandInvoker();
        _world = world ?? new WorldStateManager();
        _combatFeedback = combatFeedback ?? new CombatFeedbackManager();

        _network.GameStateReceived += OnServerStateReceived;
        _world.LocalPlayerDamaged += OnLocalPlayerDamaged;
        _world.LocalPlayerWeaponUpdated += OnLocalPlayerWeaponUpdated;
    }

    public async Task StartAsync()
    {
        ConnectionStatus = "Connecting...";
        NotificationReceived?.Invoke("Attempting connection to game server...", Color.Yellow);

        try
        {
            await _network.ConnectAsync();
            IsConnected = true;
            ConnectionStatus = "Connected";
            NotificationReceived?.Invoke("Connected to server successfully!", Color.LimeGreen);
        }
        catch (Exception ex)
        {
            IsConnected = false;
            ConnectionStatus = "Offline";
            NotificationReceived?.Invoke($"Connection failed: {ex.Message}", Color.Red);
        }
    }

    public void MoveLocalPlayer(float dx, float dy, float dt)
    {
        if (dx != 0f || dy != 0f)
        {
            _world.LocalPlayer.FacingDx = dx;
            _world.LocalPlayer.FacingDy = dy;
        }

        var moveCmd = new MoveCommand(_world.LocalPlayer, dx, dy, dt, _world.Tiles);
        _invoker.Execute(moveCmd);

        _ = _network.SendMoveAsync(
            _world.LocalPlayer.X,
            _world.LocalPlayer.Y,
            _world.LocalPlayer.FacingDx,
            _world.LocalPlayer.FacingDy);

        if (_world.MyId != -1)
            _world.Players[_world.MyId] = _world.LocalPlayer;
    }

    public void Dash()
    {
        var dashCmd = new DashCommand(_world.LocalPlayer, _world.Tiles, _network, _combatFeedback);
        _invoker.Execute(dashCmd);
        NotificationReceived?.Invoke("Dodge Roll / Dash executed!", Color.Cyan);
    }

    public void Recall()
    {
        var recallCmd = new RecallCommand(_invoker, _world.LocalPlayer, _network, _combatFeedback);
        _invoker.Execute(recallCmd);
        NotificationReceived?.Invoke("Chrono-Recall: Position rewound!", Color.Gold);
    }

    public void UsePotion()
    {
        var potionCmd = new UsePotionCommand(_world.LocalPlayer, 25f, _combatFeedback);
        _invoker.Execute(potionCmd);
        NotificationReceived?.Invoke("Consuming healing potion (+25 HP)!", Color.LimeGreen);
    }

    public void UpgradeWeapon(string upgradeType)
    {
        _ = _network.SendUpgradeAsync(upgradeType);
    }

    public void Update(float dt)
    {
        _combatFeedback.Update(dt);
    }

    public void Render(Graphics g)
    {
        _renderer.Draw(g, _world.Players.Values, _world.Enemies.Values, _world.Tiles);
        _renderer.DrawFeedback(g, _combatFeedback.GetActiveTextsSnapshot());
        _renderer.DrawHud(g, _world.LocalPlayer, ConnectionStatus, EnemyCount);
    }

    private void OnServerStateReceived(GameStatePacket packet)
    {
        _world.SyncFromPacket(packet);
        StateChanged?.Invoke();
    }

    private void OnLocalPlayerDamaged(float previousHp, float newHp, float damage)
    {
        _combatFeedback.AddText($"-{damage:0} HP", _world.LocalPlayer.X, _world.LocalPlayer.Y - 0.3f, Color.Red, 1.0f);
        NotificationReceived?.Invoke($"Took {damage:0} damage!", Color.OrangeRed);
    }

    private void OnLocalPlayerWeaponUpdated(Player player)
    {
        _combatFeedback.AddText("UPGRADED!", player.X, player.Y - 0.5f, Color.LimeGreen, 1.2f);
        NotificationReceived?.Invoke(
            $"Server confirmed upgrade: {player.WeaponName} (Dmg: {player.WeaponDamage:0.0}, Int: {player.WeaponInterval:0.00}s, Reach: {player.WeaponReach:0.00})",
            Color.LimeGreen);
    }
}
