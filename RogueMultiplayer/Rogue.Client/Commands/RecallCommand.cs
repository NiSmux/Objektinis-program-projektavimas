using System.Drawing;
using Rogue.Client.Entities;
using Rogue.Client.Networking;
using Rogue.Client.Subsystems;

namespace Rogue.Client.Commands;

/// <summary>
/// Active roguelike ability: Chrono-Recall / Time-Warp.
/// Rewinds recent player movement/actions using the Command history stack,
/// syncing the restored position to the server.
/// </summary>
public sealed class RecallCommand : ICommand
{
    private readonly CommandInvoker _invoker;
    private readonly Player _player;
    private readonly NetworkClient _network;
    private readonly CombatFeedbackManager? _feedback;

    public RecallCommand(
        CommandInvoker invoker,
        Player player,
        NetworkClient network,
        CombatFeedbackManager? feedback = null)
    {
        _invoker = invoker;
        _player = player;
        _network = network;
        _feedback = feedback;
    }

    public void Execute()
    {
        // Reverses stored commands
        _invoker.UndoLast();

        // Broadcast the restored position to authoritative server
        _ = _network.SendMoveAsync(_player.X, _player.Y, _player.FacingDx, _player.FacingDy);

        _feedback?.AddText("TIME RECALL!", _player.X, _player.Y - 0.5f, Color.Gold, 1.2f);
    }

    public void Undo()
    {
        // Recall is a terminal rewind meta-command; undoing a recall is intentionally a no-op
    }
}
