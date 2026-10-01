using Rogue.Server.Game;
using Rogue.Server.Strategy;
using Rogue.Shared.Packets;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

if (args.Contains("--strategy-demo"))
{
    StrategyDemo.Run();
    return;
}

GameServer server = new();

HttpListener listener = new();
listener.Prefixes.Add("http://localhost:8080/");
listener.Start();

Console.WriteLine("Server running");

// Background game loop: tick the spawner at ~20 ups and broadcast state.
_ = GameLoop();

while (true)
{
    var context = await listener.GetContextAsync();

    var socket =
        (await context.AcceptWebSocketAsync(null)).WebSocket;

    _ = HandleClient(socket);
}

async Task GameLoop()
{
    float tickRate = GameSettings.Instance.TickRate;
    var delay = TimeSpan.FromSeconds(tickRate);

    while (true)
    {
        server.Tick(tickRate);

        if (server.Clients.Count > 0)
            await BroadcastState();

        await Task.Delay(delay);
    }
}

async Task HandleClient(WebSocket socket)
{
    var player = server.AddPlayer(socket);
    Console.WriteLine($"Player {player.Id} joined");

    byte[] buffer = new byte[1024];

    try
    {
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
                break;

            string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            MovePacket? move = JsonSerializer.Deserialize<MovePacket>(json);
            if (move == null) continue;

            player.X = move.X;
            player.Y = move.Y;

            await BroadcastState();
        }
    }
    catch (WebSocketException)
    {
        // Connection dropped abnormally; fall through to cleanup below.
    }

    server.RemovePlayer(socket);
    Console.WriteLine($"Player {player.Id} left");

    // Let remaining clients know this player is gone.
    await BroadcastState();
}

async Task BroadcastState()
{
    var snapshot      = server.GetSnapshot();
    var enemySnapshot = server.GetEnemySnapshot();

    var sendTasks = server.Clients.Select(async kvp =>
    {
        var (socket, client) = (kvp.Key, kvp.Value);

        if (socket.State != WebSocketState.Open)
            return;

        GameStatePacket packet = new()
        {
            YourPlayerId = client.State.Id,   // personalized per recipient
            Players      = snapshot,
            Enemies      = enemySnapshot,
            Tiles = server.GetTileSnapshot()
        };

        byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(packet));

        await client.SendLock.WaitAsync();
        try
        {
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (WebSocketException)
        {
            // Socket died mid-broadcast; the owning HandleClient loop will
            // notice on its next ReceiveAsync and clean up via RemovePlayer.
        }
        finally
        {
            client.SendLock.Release();
        }
    });

    await Task.WhenAll(sendTasks);
}

