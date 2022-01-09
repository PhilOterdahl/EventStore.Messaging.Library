using EventStore.Library.Core.Event;

namespace EventStore.Library.Messaging.Event;

public abstract class AsyncEvent : EventStoreEvent, IAsyncEvent
{
    public override EventMetaData MetaData { get; set; } = new()
    {
        MessageType = "async-event"
    };

    protected AsyncEvent(string @by) : base(@by)
    {
    }
}