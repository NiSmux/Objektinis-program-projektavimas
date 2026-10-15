using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Rogue.Server.Game;
using Rogue.Shared.Packets;

namespace Rogue.Server.Observer;

/// <summary>
/// Concrete Observer in the Observer pattern.
/// Represents an active network session for a connected player.
/// When the game server's state updates, this observer pulls the latest snapshot,
/// serializes it into a personalized <see cref="GameStatePacket"/>, and broadcasts it over WebSocket.
/// </summary>
public class PlayerSessionObserver : IGameObserver
{
    private readonly WebSocket _socket;
    private readonly ConnectedClient _client;

    public int PlayerId => _client.State.Id;

    public PlayerSessionObserver(WebSocket socket, ConnectedClient client)
    {
        _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task UpdateAsync(IGameSubject subject)
    {
        if (subject is not GameServer gameServer)
            return;

        if (_socket.State != WebSocketState.Open)
            return;

        GameStatePacket packet = new()
        {
            YourPlayerId = _client.State.Id,
            Players = gameServer.GetSnapshot(),
            Enemies = gameServer.GetEnemySnapshot(),
            Tiles = gameServer.GetTileSnapshot()
        };

        byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(packet));

        await _client.SendLock.WaitAsync();
        try
        {
            if (_socket.State == WebSocketState.Open)
            {
                await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
        catch (WebSocketException)
        {
            // Socket disconnected mid-broadcast; connection handler cleans up session.
        }
        finally
        {
            _client.SendLock.Release();
        }
    }
}
