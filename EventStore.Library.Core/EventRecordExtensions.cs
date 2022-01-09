using System.Text;
using System.Text.Json;
using EventStore.Client;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Core;

public static class EventRecordExtensions
{
    public static Type GetEventType(this EventRecord @event) => EventTypes.Types[@event.EventType];

    public static TEvent ToEvent<TEvent>(this ResolvedEvent @event) where TEvent : IEventStoreEvent
    {
        var eventJson = Encoding.Default.GetString(@event.Event.Data.ToArray());
        var eventType = @event.Event.GetEventType();

        var deserializedEvent = (TEvent)JsonSerializer.Deserialize(
            eventJson,
            eventType,
            EventStoreOptions.SerializerOptions
        )!;

        deserializedEvent.Id = @event.OriginalEvent.EventId;
        return deserializedEvent;
    }

    public static EventMetaData ToEventMetaData(this ResolvedEvent @event)
    {
        var eventJson = Encoding.Default.GetString(@event.Event.Metadata.ToArray());

        var deserializedEvent = JsonSerializer.Deserialize<EventMetaData>(
            eventJson,
            EventStoreOptions.SerializerOptions
        );

        return deserializedEvent!;
    }

    public static byte[] ToEventData<TEvent>(this TEvent @event) => JsonSerializer.SerializeToUtf8Bytes(@event, EventStoreOptions.SerializerOptions);

    public static IEnumerable<TEvent> SelectEvents<TEvent>(this IEnumerable<ResolvedEvent> events)
        where TEvent : IEventStoreEvent =>
        events.Select(@event => @event.ToEvent<TEvent>());
}