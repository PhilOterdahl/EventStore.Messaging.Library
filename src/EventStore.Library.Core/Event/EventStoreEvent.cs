using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventStore.Client;
using EventStore.Library.Core.Domain.Aggregate;

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
        By = @by;
    }

   
    public string GetEventType()
    {
        var type = GetType();
        var eventAttribute = type.GetCustomAttribute<EventAttribute>();
        var typeName = eventAttribute?.Name ?? type.Name;
        var version = eventAttribute?.Version;
        return version is not null 
            ? $"{typeName}-v{version}" 
            : typeName;
    }

    public byte[] ToData() => JsonSerializer.SerializeToUtf8Bytes(this, GetType(), EventStoreOptions.SerializerOptions);

    protected ReadOnlyMemory<byte>? GetMetaData() => JsonSerializer.SerializeToUtf8Bytes(ToMetaData(), EventStoreOptions.SerializerOptions);

    public EventData ToDataModel() => new(Id, GetEventType(), ToData(), GetMetaData());

    public EventMetaData ToMetaData() => MetaData;
}