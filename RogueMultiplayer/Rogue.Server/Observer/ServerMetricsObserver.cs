using Rogue.Server.Game;

namespace Rogue.Server.Observer;

/// <summary>
/// Concrete Observer in the Observer pattern.
/// Monitors server performance, player population, and active enemy counts.
/// Decouples telemetry and diagnostic logging from the core <see cref="GameServer"/> game loop.
/// </summary>
public class ServerMetricsObserver : IGameObserver
{
    private int _tickCount;
    private int _lastPlayerCount = -1;
    private int _lastEnemyCount = -1;
    private readonly int _reportIntervalTicks;

    public int TotalUpdatesObserved => _tickCount;

    public ServerMetricsObserver(int reportIntervalTicks = 100)
    {
        _reportIntervalTicks = reportIntervalTicks;
    }

    public Task UpdateAsync(IGameSubject subject)
    {
        if (subject is not GameServer gameServer)
            return Task.CompletedTask;

        _tickCount++;

        int currentPlayerCount = gameServer.Clients.Count;
        int currentEnemyCount = gameServer.Spawner.Enemies.Count;

        bool populationChanged = currentPlayerCount != _lastPlayerCount || currentEnemyCount != _lastEnemyCount;
        bool intervalElapsed = _tickCount % _reportIntervalTicks == 0;

        if (populationChanged || intervalElapsed)
        {
            _lastPlayerCount = currentPlayerCount;
            _lastEnemyCount = currentEnemyCount;

            Console.WriteLine(
                $"[MetricsObserver] Tick #{_tickCount} | Connected Players: {currentPlayerCount} | Active Enemies: {currentEnemyCount}");
        }

        return Task.CompletedTask;
    }
}
