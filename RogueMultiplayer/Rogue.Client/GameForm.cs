using Rogue.Client.Entities;
using Rogue.Client.Input;
using Rogue.Client.Networking;
using Rogue.Client.Rendering;
using Rogue.Shared.Packets;
using System.Numerics;

namespace Rogue.Client;

public partial class GameForm : Form
{
    private readonly Dictionary<int, Player> players = new();

    private Player localPlayer = new();
    private int myId = -1;
    private readonly Renderer renderer = new();
    private readonly InputManager input = new();
    private readonly NetworkClient network = new();

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

        if (input.Up)
            localPlayer.Y -= localPlayer.Speed * dt;

        if (input.Down)
            localPlayer.Y += localPlayer.Speed * dt;

        if (input.Left)
            localPlayer.X -= localPlayer.Speed * dt;

        if (input.Right)
            localPlayer.X += localPlayer.Speed * dt;

        localPlayer.X = Math.Clamp(
            localPlayer.X,
            0,
            Renderer.GridSize - 1);

        localPlayer.Y = Math.Clamp(
            localPlayer.Y,
            0,
            Renderer.GridSize - 1);

        _ = network.SendMoveAsync(
            localPlayer.X,
            localPlayer.Y);

        if (myId != -1)
            players[myId] = localPlayer;

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        renderer.Draw(e.Graphics, players.Values);
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

        players.Clear();

        foreach (var state in packet.Players)
        {
            players[state.Id] = new Player
            {
                X = state.X,
                Y = state.Y
            };
        }

        myId = packet.YourPlayerId;

        if (players.TryGetValue(myId, out Player? player))
        {
            localPlayer.X = player.X;
            localPlayer.Y = player.Y;
        }

        Invalidate();
    }
}