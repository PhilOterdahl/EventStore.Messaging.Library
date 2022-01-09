namespace EventStore.Library.Core.Domain.Aggregate;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class EventAttribute : Attribute
{
    public int Version { get; set; }
    public string? Name { get; set; }
}