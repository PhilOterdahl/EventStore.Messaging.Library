using System.Collections.Concurrent;
using System.Text.Json;
using EventStore.Client;

namespace EventStore.Library.Core;

public class EventStoreOptions
{
    public static JsonSerializerOptions SerializerOptions { get; private set; } = new(JsonSerializerDefaults.General);
    public EventStoreClientOptions ClientOptions { get; }
    public EventStoreClientOperationOptions ClientOperationOptions { get; }
 

    public EventStoreOptions(
        EventStoreClientOptions clientOptions,
        EventStoreClientOperationOptions? clientOperationOptions = null, 
        JsonSerializerOptions? serializerOptions = null)
    {
        ClientOptions = clientOptions;
        SerializerOptions = serializerOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.General);
        ClientOperationOptions = clientOperationOptions ?? EventStoreClientOperationOptions.Default;
    }
}

public class EventStoreClientOptions
{
    public string ConnectionStringGrcp { get; set; }
    public string ConnectionStringHttp { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}