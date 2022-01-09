using EventStore.Library.Core;

namespace EventStore.Library.Messaging;

internal class AsyncCommandStreamId : StreamId
{
    public AsyncCommandStreamId() : base("async-command")
    {
    }

    public override string Category => "messages";
}