using System.Collections.Concurrent;
using System.Text.Json;
using EventStore.Library.Messaging.Query;

namespace EventStore.Library.Examples.Messaging.Behaviours;

internal static class Cache
{
    private static readonly ConcurrentDictionary<string, object> CachedQueryResponses = new();

    public static bool IsCached(string cacheKey) => CachedQueryResponses.ContainsKey(cacheKey);
    public static object GetCachedQueryResponse(string cacheKey) => CachedQueryResponses[cacheKey];
    public static object AddQueryResponse(string cacheKey, object queryResponse) => CachedQueryResponses.AddOrUpdate(cacheKey, id => queryResponse, (id, cachedValue) => queryResponse);
}

internal class CachingQueryPipelineBehavior<TQuery, TResponse> : IQueryPipelineBehavior<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly ILogger _logger;

    public CachingQueryPipelineBehavior(ILogger<CachingQueryPipelineBehavior<TQuery, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TQuery query, QueryHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(query.GetType(), query);

        var (response, loadedFromCache) = Cache.IsCached(cacheKey)
            ? ((TResponse)Cache.GetCachedQueryResponse(cacheKey), true)
            : (await next(), false);

        if (response is null)
            return response;

        if (!loadedFromCache)
        {
            Cache.AddQueryResponse(cacheKey, response);
            _logger.LogInformation($"Query: {query.GetType().Name} was added to cache");
            return response;
        }
        
        _logger.LogInformation($"Query: {query.GetType().Name} was loaded from cache");
        return response;
    }


    private static string GetCacheKey(Type queryType, object query) =>
        JsonSerializer.Serialize(new Dictionary<string, object>
        {
            { queryType.FullName, query }
        });
}