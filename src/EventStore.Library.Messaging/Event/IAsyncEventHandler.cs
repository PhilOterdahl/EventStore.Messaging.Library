namespace EventStore.Library.Messaging.Event;

/// <summary>Defines a handler for a async event</summary>
/// <typeparam name="TEvent">The type of event being handled</typeparam>
public interface IAsyncEventHandler<in TEvent> where TEvent : IAsyncEvent
{
    /// <summary>Handles a event</summary>
    /// <param name="event">The event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Handle(TEvent @event, CancellationToken cancellationToken);
}