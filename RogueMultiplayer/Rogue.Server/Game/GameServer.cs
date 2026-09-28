using System.Collections.Concurrent;
using System.Net.WebSockets;
using Rogue.Server.Enemies;
using Rogue.Shared.Models;

namespace Rogue.Server.Game;

public class ConnectedClient
{
    public PlayerState State { get; }
    public SemaphoreSlim SendLock { get; } = new(1, 1);

    public ConnectedClient(PlayerState state) => State = state;
}

public class GameServer
{
    public ConcurrentDictionary<WebSocket, ConnectedClient> Clients { get; } = new();

    private int nextId = 1;

    public EnemySpawner Spawner { get; } = new(new EnemyFactory());

    public PlayerState AddPlayer(WebSocket socket)
    {
        PlayerState player = new()
        {
            Id = Interlocked.Increment(ref nextId) - 1,
            X = 3,
            Y = 3
        };

        Clients[socket] = new ConnectedClient(player);

        return player;
    }

    public void RemovePlayer(WebSocket socket)
    {
        Clients.TryRemove(socket, out _);
    }

    /// <summary>Advances the enemy spawner by <paramref name="deltaTime"/> seconds.</summary>
    public void Tick(float deltaTime) => Spawner.Update(deltaTime);

    public List<PlayerState> GetSnapshot()
    {
        return Clients.Values.Select(c => c.State).ToList();
    }

    public List<EnemyState> GetEnemySnapshot() => Spawner.GetSnapshot();
}