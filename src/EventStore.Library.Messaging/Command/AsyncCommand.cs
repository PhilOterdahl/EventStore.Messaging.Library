using EventStore.Library.Core.Event;

namespace EventStore.Library.Messaging.Command;

public abstract class AsyncCommand : EventStoreEvent, IAsyncCommand
{
    protected AsyncCommand(string by) : base(by)
    {
        MetaData = new EventMetaData
        {
            MessageType = "async-command"
        };
    }
}