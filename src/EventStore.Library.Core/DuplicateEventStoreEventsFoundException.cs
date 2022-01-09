namespace EventStore.Library.Core;

internal class DuplicateEventStoreEventsFoundException : InvalidOperationException
{
    public DuplicateEventStoreEventsFoundException(string eventName) : 
        base($"Duplicate event store events found: {eventName}, either change name of event or add a version using EventStoreEventAttribute")
    {
            
    }
}