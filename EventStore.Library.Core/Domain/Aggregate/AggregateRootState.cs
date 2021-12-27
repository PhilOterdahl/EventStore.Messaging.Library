namespace EventStore.Library.Core.Domain.Aggregate;

public abstract class AggregateRootState
{
    public DateTime LastUpdated { get; set; }
    public DateTime Created { get; set; }
    public string? LastUpdatedBy { get; set; }
}