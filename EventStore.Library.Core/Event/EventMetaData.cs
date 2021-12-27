using System.Text.Json.Serialization;

namespace EventStore.Library.Core.Event;

public class EventMetaData
{
    [JsonPropertyName("$correlationId")]
    public string? CorrelationId { get; set; }
    [JsonPropertyName("$causationId")]
    public string? CausationId { get; set; }
    public string? MessageType { get; set; }
    public bool ShouldProcess { get; set; } = true;
}