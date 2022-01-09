namespace EventStore.Library.Messaging;

internal class NoEventHandlerRegisteredException : InvalidOperationException
{
    public NoEventHandlerRegisteredException(string eventType) : base($"No event handler registered for event: {eventType}")
    {
            
    }
}