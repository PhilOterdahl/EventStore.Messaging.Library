using EventStore.Library.Core;

namespace EventStore.Library.Messaging;

internal class AsyncEventStreamId : StreamId
{
    public AsyncEventStreamId() : base("async-event")
    {
    }

    public override string Category => "messages";
}