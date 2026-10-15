using Rogue.Server.Game;

namespace Rogue.Server.Observer;

/// <summary>
/// Concrete Observer in the Observer pattern.
/// Observes player and enemy health transitions and logs combat events
/// (damage taken, enemy defeats) to the server console.
/// </summary>
public class CombatLogObserver : IGameObserver
{
    private readonly Dictionary<int, float> _playerHealthHistory = new();
    private readonly HashSet<int> _activeEnemyIds = new();

    public Task UpdateAsync(IGameSubject subject)
    {
        if (subject is not GameServer gameServer)
            return Task.CompletedTask;

        // 1. Observe Player Health Changes
        var currentPlayers = gameServer.GetSnapshot();
        foreach (var player in currentPlayers)
        {
            if (_playerHealthHistory.TryGetValue(player.Id, out float previousHealth))
            {
                if (player.Health < previousHealth)
                {
                    float damage = previousHealth - player.Health;
                    Console.WriteLine(
                        $"[CombatObserver] Player {player.Id} took {damage:0} damage! (Health: {player.Health:0}/{player.MaxHealth:0})");
                }
            }
            _playerHealthHistory[player.Id] = player.Health;
        }

        // Clean up disconnected players from history
        var activePlayerIds = currentPlayers.Select(p => p.Id).ToHashSet();
        var departedPlayerIds = _playerHealthHistory.Keys.Where(id => !activePlayerIds.Contains(id)).ToList();
        foreach (var id in departedPlayerIds)
        {
            _playerHealthHistory.Remove(id);
        }

        // 2. Observe Enemy Defeats
        var currentEnemies = gameServer.GetEnemySnapshot();
        var currentEnemyIds = currentEnemies.Select(e => e.Id).ToHashSet();

        foreach (var previousEnemyId in _activeEnemyIds)
        {
            if (!currentEnemyIds.Contains(previousEnemyId))
            {
                Console.WriteLine($"[CombatObserver] Enemy #{previousEnemyId} was defeated!");
            }
        }

        _activeEnemyIds.Clear();
        foreach (var id in currentEnemyIds)
        {
            _activeEnemyIds.Add(id);
        }

        return Task.CompletedTask;
    }
}
