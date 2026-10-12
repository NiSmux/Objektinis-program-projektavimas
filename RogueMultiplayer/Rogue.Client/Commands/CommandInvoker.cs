namespace Rogue.Client.Commands;

/// <summary>
/// Executes <see cref="ICommand"/> objects and maintains a bounded history
/// so that <see cref="UndoLast"/> can reverse all stored commands at once,
/// snapping the player back by up to <see cref="HistoryCapacity"/> frames
/// of movement (≈ 1 second at 60 FPS) — making the Undo effect clearly visible.
/// </summary>
public sealed class CommandInvoker
{
    /// <summary>
    /// Number of frames kept in history.  At 60 FPS this equals ~1 second of movement.
    /// </summary>
    public const int HistoryCapacity = 60;

    // Oldest command is at the bottom; newest is at the top (standard stack order).
    private readonly Stack<ICommand> _history = new(HistoryCapacity);

    /// <summary>
    /// Runs <paramref name="command"/> and pushes it onto the undo history.
    /// Once the history reaches <see cref="HistoryCapacity"/> the oldest entry
    /// is discarded to keep memory use bounded.
    /// </summary>
    public void Execute(ICommand command)
    {
        command.Execute();

        if (_history.Count == HistoryCapacity)
        {
            // Trim the oldest entry: copy to a list, remove the bottom item, rebuild.
            var entries = _history.ToArray();          // newest → oldest
            _history.Clear();
            for (int i = entries.Length - 2; i >= 0; i--)   // skip oldest (last element)
                _history.Push(entries[i]);
        }

        _history.Push(command);
    }

    /// <summary>
    /// Undoes every command in the history (newest first), then clears the history.
    /// The player is snapped back to the position held before the oldest stored command.
    /// Has no effect if no commands have been executed yet.
    /// </summary>
    public void UndoLast()
    {
        while (_history.Count > 0)
            _history.Pop().Undo();
    }
}
