using Rogue.Server.Enemies;
using Rogue.Server.Game;
using Rogue.Server.Observer;
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

if (args.Contains("--prototype-demo"))
{
    PrototypeDemo.Run();
    return;
}

GameServer server = new();

// Attach persistent server-side observers (Observer pattern)
server.Attach(new ServerMetricsObserver(reportIntervalTicks: 100));
server.Attach(new CombatLogObserver());

HttpListener listener = new();
listener.Prefixes.Add("http://localhost:8080/");
listener.Start();

Console.WriteLine("Server running");

// Background game loop: tick the spawner at ~20 ups and notify observers.
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
            await server.NotifyAsync();

        await Task.Delay(delay);
    }
}

async Task HandleClient(WebSocket socket)
{
    var player = server.AddPlayer(socket);
    var client = server.Clients[socket];

    // Create and attach observer for this player's network session (Observer pattern)
    var sessionObserver = new PlayerSessionObserver(socket, client);
    server.Attach(sessionObserver);

    Console.WriteLine($"Player {player.Id} joined");

    // Immediately notify all observers of the new player's arrival
    await server.NotifyAsync();

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

            // Preserve last facing if client stopped moving (dx=0, dy=0)
            if (move.FacingDx != 0f || move.FacingDy != 0f)
            {
                player.FacingDx = move.FacingDx;
                player.FacingDy = move.FacingDy;
            }

            await server.NotifyAsync();
        }
    }
    catch (WebSocketException)
    {
        // Connection dropped abnormally; fall through to cleanup below.
    }
    finally
    {
        // Detach observer and clean up session (Observer pattern)
        server.Detach(sessionObserver);
        server.RemovePlayer(socket);
        Console.WriteLine($"Player {player.Id} left");

        // Notify remaining observers that player left
        await server.NotifyAsync();
    }
}

