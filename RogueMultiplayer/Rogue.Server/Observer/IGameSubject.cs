namespace Rogue.Server.Observer;

/// <summary>
/// Subject (Observable) interface in the Gang of Four (GoF) Observer pattern.
/// Defines contracts for registering, unregistering, and notifying observers.
/// </summary>
public interface IGameSubject
{
    /// <summary>
    /// Attaches an observer to receive state change notifications.
    /// </summary>
    void Attach(IGameObserver observer);

    /// <summary>
    /// Detaches an observer so it no longer receives notifications.
    /// </summary>
    void Detach(IGameObserver observer);

    /// <summary>
    /// Notifies all attached observers that the subject's state has changed.
    /// </summary>
    Task NotifyAsync();
}
