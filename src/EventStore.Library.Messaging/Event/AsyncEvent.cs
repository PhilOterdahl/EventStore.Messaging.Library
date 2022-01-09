using EventStore.Library.Core.Event;

namespace EventStore.Library.Messaging.Event;

public abstract class AsyncEvent : EventStoreEvent, IAsyncEvent
{
    protected AsyncEvent(string @by) : base(@by)
    {
        MetaData = new EventMetaData
        {
            MessageType = "async-event"
        };
    }
}