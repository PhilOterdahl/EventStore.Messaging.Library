namespace EventStore.Library.Messaging.Query;

internal interface IQueryPipelineHandler<TResponse>
{
    public Task<TResponse> Handle(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}