using Rogue.Client.Entities;
using Rogue.Client.Input;
using Rogue.Client.Networking;
using Rogue.Client.Rendering;
using Rogue.Shared.Models;
using Rogue.Shared.Packets;
using System.Numerics;

namespace Rogue.Client;

public partial class GameForm : Form
{
    private readonly Dictionary<int, Player> players = new();
    private readonly Dictionary<int, EnemyState> enemies = new();

    private Player localPlayer = new();
    private int myId = -1;
    private readonly Renderer renderer = new();
    private readonly InputManager input = new();
    private readonly NetworkClient network = new();
    private List<TileState> tiles = new();

    private readonly System.Windows.Forms.Timer gameTimer = new();

    public GameForm()
    {
        InitializeComponent();
        network.GameStateReceived += OnGameStateReceived;

        _ = ConnectToServer();

        DoubleBuffered = true;
        KeyPreview = true;

        ClientSize = new Size(
            Renderer.GridSize * Renderer.TileSize,
            Renderer.GridSize * Renderer.TileSize);

        KeyDown += (s, e) => input.KeyDown(e.KeyCode);
        KeyUp += (s, e) => input.KeyUp(e.KeyCode);

        gameTimer.Interval = 16;      // ~60 FPS
        gameTimer.Tick += GameLoop;
        gameTimer.Start();
    }

    private void GameLoop(object? sender, EventArgs e)
    {
        float dt = 1f / 60f;

        float dx = (input.Right ? 1 : 0) - (input.Left ? 1 : 0);
        float dy = (input.Down ? 1 : 0) - (input.Up ? 1 : 0);

        float maxPos = Renderer.GridSize - 1;

        float newX = Math.Clamp(localPlayer.X + dx * localPlayer.Speed * dt, 0, maxPos);
        float newY = Math.Clamp(localPlayer.Y + dy * localPlayer.Speed * dt, 0, maxPos);

        // Check each axis separately so the player slides along walls
        if (!TileCollision.IsBlocked(tiles, newX, localPlayer.Y))
            localPlayer.X = newX;

        if (!TileCollision.IsBlocked(tiles, localPlayer.X, newY))
            localPlayer.Y = newY;

        _ = network.SendMoveAsync(
            localPlayer.X,
            localPlayer.Y);

        if (myId != -1)
            players[myId] = localPlayer;

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        renderer.Draw(e.Graphics, players.Values, enemies.Values, tiles);
    }

    private async Task ConnectToServer()
    {
        try
        {
            await network.ConnectAsync();
            Text = "Connected";
        }
        catch
        {
            Text = "Server not found";
        }
    }


    private void OnGameStateReceived(GameStatePacket packet)
    {
        if (InvokeRequired)
        {
            Invoke(() => OnGameStateReceived(packet));
            return;
        }
        tiles = packet.Tiles;

        players.Clear();
        enemies.Clear();

        foreach (var state in packet.Players)
        {
            players[state.Id] = new Player
            {
                X = state.X,
                Y = state.Y
            };
        }

        foreach (var e in packet.Enemies)
            enemies[e.Id] = e;

        myId = packet.YourPlayerId;

        if (players.TryGetValue(myId, out Player? player))
        {
            localPlayer.X = player.X;
            localPlayer.Y = player.Y;
        }

        Invalidate();
    }
}