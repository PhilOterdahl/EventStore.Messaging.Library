using System.Text;
using System.Text.Json;
using EventStore.Client;
using EventStore.Library.Core;
using EventStore.Library.Messaging.Command;
using EventStore.Library.Messaging.Event;

namespace EventStore.Library.Messaging;

internal static class EventRecordExtensions
{
    public static IAsyncEvent ToAsyncEvent(this ResolvedEvent @event)
    {
        var eventJson = Encoding.Default.GetString(@event.Event.Data.ToArray());
        var eventType = @event.Event.GetEventType();

        var deserializedEvent = (IAsyncEvent)JsonSerializer.Deserialize(
            eventJson,
            eventType,
            EventStoreOptions.SerializerOptions
        )!;

        deserializedEvent.Id = @event.Event.EventId;
        return deserializedEvent;
    }

    public static IAsyncCommand ToAsyncCommand(this ResolvedEvent @event)
    {
        var eventJson = Encoding.Default.GetString(@event.Event.Data.ToArray());
        var eventType = @event.Event.GetEventType();

        var deserializedEvent = (IAsyncCommand)JsonSerializer.Deserialize(
            eventJson,
            eventType,
            EventStoreOptions.SerializerOptions
        )!;

        deserializedEvent.Id = @event.Event.EventId;
        return deserializedEvent;
    }
}