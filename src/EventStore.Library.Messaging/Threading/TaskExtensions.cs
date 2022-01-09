using System.Collections.Concurrent;

namespace EventStore.Library.Messaging.Threading
{
    internal static class TaskExtensions
    {
        public static async Task<IEnumerable<T>> ToEnumerableAsyncParallel<T>(
            this IEnumerable<Task<T>> tasks,
            int? maxDegreeOfParallelism = null,
            CancellationToken cancellationToken = default)
        {
            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = maxDegreeOfParallelism ?? Environment.ProcessorCount / 2
            };
            var results = new ConcurrentBag<T>();

            await Parallel.ForEachAsync(tasks, parallelOptions, async (task, token) =>
            {
                var result = await task;
                results.Add(result);
            });

            return results.AsEnumerable();
        }

        public static async Task<T[]> ToArrayAsyncParallel<T>(
            this IEnumerable<Task<T>> tasks,
            int? maxDegreeOfParallelism = null,
            CancellationToken cancellationToken = default)
        {
            var results = await tasks.ToEnumerableAsyncParallel(maxDegreeOfParallelism, cancellationToken);
            return results.ToArray();
        }
    }
}
