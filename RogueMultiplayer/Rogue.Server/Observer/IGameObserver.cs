namespace Rogue.Server.Observer;

/// <summary>
/// Observer interface in the Gang of Four (GoF) Observer pattern.
/// Defines the update contract called by <see cref="IGameSubject"/> whenever state changes.
/// </summary>
public interface IGameObserver
{
    /// <summary>
    /// Invoked by the subject when its internal state changes.
    /// Observers pull required state from the subject.
    /// </summary>
    /// <param name="subject">The observable subject reporting the change.</param>
    Task UpdateAsync(IGameSubject subject);
}
