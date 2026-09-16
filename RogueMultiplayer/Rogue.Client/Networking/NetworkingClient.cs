using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Rogue.Shared.Packets;

namespace Rogue.Client.Networking;

public class NetworkClient
{
    private readonly ClientWebSocket socket = new();

    public event Action<GameStatePacket>? GameStateReceived;

    public async Task ConnectAsync()
    {
        await socket.ConnectAsync(
            new Uri("ws://localhost:8080/"),
            CancellationToken.None);

        _ = ReceiveLoop();
    }

    private readonly SemaphoreSlim sendLock = new(1, 1);

    public async Task SendMoveAsync(float x, float y)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new MovePacket { X = x, Y = y }));
        await sendLock.WaitAsync();
        try
        {
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        finally
        {
            sendLock.Release();
        }
    }

    private async Task ReceiveLoop()
    {
        byte[] buffer = new byte[4096];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(
                    buffer,
                    CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                string json = Encoding.UTF8.GetString(
                    buffer,
                    0,
                    result.Count);

                GameStatePacket? packet =
                    JsonSerializer.Deserialize<GameStatePacket>(json);

                if (packet != null)
                {
                    GameStateReceived?.Invoke(packet);
                }
            }
        }
        catch (WebSocketException)
        {
            Console.WriteLine("WebSocket connection lost.");
        }
    }
}