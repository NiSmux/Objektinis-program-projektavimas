namespace Rogue.Client.Commands;

/// <summary>
/// Abstraction for all reversible game commands (Command Design Pattern).
/// </summary>
public interface ICommand
{
    /// <summary>Carries out the command's effect.</summary>
    void Execute();

    /// <summary>
    /// Reverses the local effect produced by the most recent <see cref="Execute"/> call.
    /// </summary>
    /// <remarks>
    /// In a networked game, Undo() restores client-side state only.
    /// The server is not notified; it will reconcile position on the next
    /// authoritative broadcast.
    /// </remarks>
    void Undo();
}
