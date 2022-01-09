using EventStore.Library.Core;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Messaging.Event;

public abstract class AsyncDomainEvent<TId> : DomainEvent<TId>, IAsyncEvent
    where TId : StreamId
{
    protected AsyncDomainEvent(TId streamId, string @by) : base(streamId, @by)
    {
        MetaData = new EventMetaData
        {
            MessageType = "async-event"
        };
    }
}