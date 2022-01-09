namespace EventStore.Library.Messaging.Query;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>Handles a query</summary>
    /// <param name="query">The query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response from the query</returns>
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}