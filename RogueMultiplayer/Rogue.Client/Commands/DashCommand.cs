using System.Drawing;
using Rogue.Client.Entities;
using Rogue.Client.Networking;
using Rogue.Client.Rendering;
using Rogue.Client.Subsystems;
using Rogue.Shared.Models;

namespace Rogue.Client.Commands;

/// <summary>
/// Active roguelike ability: Quick burst dash / dodge roll in the player's facing direction.
/// Fully reversible with Undo() (Command pattern).
/// </summary>
public sealed class DashCommand : ICommand
{
    private readonly Player _player;
    private readonly IReadOnlyList<TileState> _tiles;
    private readonly NetworkClient _network;
    private readonly CombatFeedbackManager? _feedback;

    private float _prevX;
    private float _prevY;
    private bool _executed;

    private const float DashDistance = 2.0f;
    private const float MaxPos = Renderer.GridSize - 1;

    public DashCommand(
        Player player,
        IReadOnlyList<TileState> tiles,
        NetworkClient network,
        CombatFeedbackManager? feedback = null)
    {
        _player = player;
        _tiles = tiles;
        _network = network;
        _feedback = feedback;
    }

    public void Execute()
    {
        _prevX = _player.X;
        _prevY = _player.Y;
        _executed = true;

        float dirX = _player.FacingDx;
        float dirY = _player.FacingDy;

        // Normalize direction if non-zero
        float len = MathF.Sqrt(dirX * dirX + dirY * dirY);
        if (len > 0.001f)
        {
            dirX /= len;
            dirY /= len;
        }
        else
        {
            dirX = 1f;
            dirY = 0f;
        }

        float newX = Math.Clamp(_player.X + dirX * DashDistance, 0f, MaxPos);
        float newY = Math.Clamp(_player.Y + dirY * DashDistance, 0f, MaxPos);

        // Slide along walls if diagonal, or apply collision check
        if (!TileCollision.IsBlocked(_tiles, newX, _player.Y))
            _player.X = newX;

        if (!TileCollision.IsBlocked(_tiles, _player.X, newY))
            _player.Y = newY;

        _ = _network.SendMoveAsync(_player.X, _player.Y, _player.FacingDx, _player.FacingDy);
        _feedback?.AddText("DASH!", _player.X, _player.Y - 0.5f, Color.Cyan, 0.8f);
    }

    public void Undo()
    {
        if (!_executed) return;

        _player.X = _prevX;
        _player.Y = _prevY;

        _ = _network.SendMoveAsync(_player.X, _player.Y, _player.FacingDx, _player.FacingDy);
        _feedback?.AddText("DASH UNDONE", _player.X, _player.Y - 0.5f, Color.DarkTurquoise, 0.8f);
    }
}
