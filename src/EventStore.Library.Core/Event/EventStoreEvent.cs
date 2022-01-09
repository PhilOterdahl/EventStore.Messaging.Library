using System.Text.Json;
using System.Text.Json.Serialization;
using EventStore.Client;

namespace EventStore.Library.Core.Event;

public class EventStoreEvent : IEventStoreEvent
{
    [JsonIgnore]
    public Uuid Id { get; set; } = Uuid.NewUuid();
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string By { get; set; }

    [JsonIgnore]
    protected EventMetaData MetaData = new();

    public EventStoreEvent(string @by)
    {
        By = @by ?? throw new ArgumentNullException(nameof(@by));
    }

    public string GetEventType() => GetType().GetEventType();

    public byte[] ToData() => JsonSerializer.SerializeToUtf8Bytes(this, GetType(), EventStoreOptions.SerializerOptions);

    protected ReadOnlyMemory<byte>? GetMetaData() => JsonSerializer.SerializeToUtf8Bytes(ToMetaData(), EventStoreOptions.SerializerOptions);

    public EventData ToDataModel() => new(Id, GetEventType(), ToData(), GetMetaData());

    public EventMetaData ToMetaData() => MetaData;
}