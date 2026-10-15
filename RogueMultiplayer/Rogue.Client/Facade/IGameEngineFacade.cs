using System.Drawing;
using Rogue.Client.Entities;
using Rogue.Shared.Models;

namespace Rogue.Client.Facade;

/// <summary>
/// Facade interface providing a unified, simplified entry point for client UI and controller
/// components, hiding the complexity of networking, rendering, command execution,
/// entity synchronization, and visual combat feedback.
/// </summary>
public interface IGameEngineFacade
{
    // Lifecycle & Networking
    Task StartAsync();
    string ConnectionStatus { get; }
    bool IsConnected { get; }

    // State Inspection
    Player LocalPlayer { get; }
    int LocalPlayerId { get; }
    int EnemyCount { get; }
    int PlayerCount { get; }
    IReadOnlyList<TileState> Tiles { get; }

    // High-Level Gameplay Operations (Command dispatch)
    void MoveLocalPlayer(float dx, float dy, float dt);
    void Dash();
    void Recall();
    void UsePotion();

    // Loop & Rendering Operations
    void Update(float dt);
    void Render(Graphics g);

    // Facade Events
    event Action? StateChanged;
    event Action<string, Color>? NotificationReceived;
}
