using Rogue.Client.Entities;
using Rogue.Client.Rendering;
using Rogue.Shared.Models;

namespace Rogue.Client.Commands;

/// <summary>
/// Moves a <see cref="Player"/> by one game-loop tick in response to WASD input.
///
/// Execute() applies the same slide-along-wall, boundary-clamped movement that
/// previously lived inline in GameForm.GameLoop().
///
/// Undo() restores the position that existed immediately before Execute() ran.
/// NOTE: Undo() is local-only. In the current multiplayer architecture the server
/// is not informed; the server will reconcile on its next authoritative broadcast.
/// </summary>
public sealed class MoveCommand : ICommand
{
    // ── Dependencies captured at construction time ──────────────────────────────
    private readonly Player _player;
    private readonly float _dx;          // raw direction component: −1, 0, or +1
    private readonly float _dy;
    private readonly float _dt;          // delta-time in seconds
    private readonly IReadOnlyList<TileState> _tiles;

    // ── Memento fields (written by Execute, read by Undo) ───────────────────────
    private float _prevX;
    private float _prevY;
    private bool _executed;

    private const float MaxPos = Renderer.GridSize - 1;

    /// <param name="player">The player to move.</param>
    /// <param name="dx">Horizontal direction: (Right?1:0)−(Left?1:0).</param>
    /// <param name="dy">Vertical direction:   (Down?1:0)−(Up?1:0).</param>
    /// <param name="dt">Frame delta-time in seconds.</param>
    /// <param name="tiles">Current tile snapshot used for wall collision.</param>
    public MoveCommand(Player player, float dx, float dy, float dt, IReadOnlyList<TileState> tiles)
    {
        _player = player;
        _dx     = dx;
        _dy     = dy;
        _dt     = dt;
        _tiles  = tiles;
    }

    /// <inheritdoc/>
    public void Execute()
    {
        // 1. Snapshot position so Undo() can restore it.
        _prevX    = _player.X;
        _prevY    = _player.Y;
        _executed = true;

        // 2. Guard: nothing to do when the player is standing still.
        if (_dx == 0f && _dy == 0f)
            return;

        // 3. Compute candidate positions.
        float newX = Math.Clamp(_player.X + _dx * _player.Speed * _dt, 0f, MaxPos);
        float newY = Math.Clamp(_player.Y + _dy * _player.Speed * _dt, 0f, MaxPos);

        // 4. Apply each axis independently so the player slides along walls
        //    rather than stopping dead on diagonal contact.
        if (!TileCollision.IsBlocked(_tiles, newX, _player.Y))
            _player.X = newX;

        if (!TileCollision.IsBlocked(_tiles, _player.X, newY))
            _player.Y = newY;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Restores the client-side position to what it was before <see cref="Execute"/> ran.
    /// Does not communicate with the server.
    /// </remarks>
    public void Undo()
    {
        if (!_executed)
            return;

        _player.X = _prevX;
        _player.Y = _prevY;
    }
}
