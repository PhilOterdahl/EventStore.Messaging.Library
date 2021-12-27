using System.Text.Json;
using System.Text.Json.Serialization;
using EventStore.Client;

namespace EventStore.Library.Core.Event;

public class EventStoreEvent : IEventStoreEvent
{
    public Uuid Id { get; set; } = Uuid.NewUuid();
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string By { get; set; }
    public string EventType => GetType().Name;

    [JsonIgnore]
    public virtual EventMetaData MetaData { get; set; } = new();

    public EventStoreEvent(string @by)
    {
        By = @by;
    }

    public byte[] ToData() => JsonSerializer.SerializeToUtf8Bytes(this, EventStoreOptions.SerializerOptions);

    protected ReadOnlyMemory<byte>? GetMetaData() => JsonSerializer.SerializeToUtf8Bytes(MetaData);

    public EventData ToDataModel() => new(Id, EventType, ToData(), GetMetaData());
}