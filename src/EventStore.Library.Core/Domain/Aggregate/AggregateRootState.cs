namespace EventStore.Library.Core.Domain.Aggregate;

public abstract class AggregateRootState<TId> where TId : IStreamId
{
    public TId? Id { get; set; }
    public DateTime? LastUpdated { get; set; }
    public DateTime? Created { get; set; }
    public string? LastUpdatedBy { get; set; }
}