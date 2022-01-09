namespace EventStore.Library.Core.Domain.Aggregate;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class EventStoreEventAttribute : Attribute
{
    public int Version { get; set; } = 0;
    public string? Name { get; set; }
}