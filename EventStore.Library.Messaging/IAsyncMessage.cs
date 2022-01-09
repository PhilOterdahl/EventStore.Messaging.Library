using EventStore.Library.Core.Event;

namespace EventStore.Library.Messaging;

public interface IAsyncMessage : IEventStoreEvent, IMessage
{
}