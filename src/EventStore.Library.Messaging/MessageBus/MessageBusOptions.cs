using EventStore.Client;

namespace EventStore.Library.Messaging.MessageBus;

public class MessageBusOptions
{
    public MessageStatusRepositoryType MessageStatusRepositoryType { get; private set; } = MessageStatusRepositoryType.InMemory;
    public AsyncCommandProcessingOptions AsyncCommandProcessingOptions { get; } = new();
    public AsyncEventProcessingOptions AsyncEventProcessingOptions { get; } = new();

    public MessageBusOptions UseInMemoryMessageStatusStorage()
    {
        MessageStatusRepositoryType = MessageStatusRepositoryType.InMemory;
        return this;
    }

    public MessageBusOptions UseSqlServerMessageStatusStorage()
    {
        MessageStatusRepositoryType = MessageStatusRepositoryType.SqlServer;
        return this;
    }
}

public enum MessageStatusRepositoryType
{
    InMemory,
    SqlServer,
}

public class AsyncCommandProcessingOptions
{
    public TimeSpan MaxAge { get; private set; } = TimeSpan.FromDays(30);
    public TimeSpan MessageTimeout { get; private set; } =  TimeSpan.FromSeconds(30);
    public int MaxRetryCount { get; private set; } = 10;
    public string ConsumerStrategy { get; private set; } = SystemConsumerStrategies.RoundRobin;
    public int MaxDegreeOfParallelism { get; private set; } = Environment.ProcessorCount / 2;

    public AsyncCommandProcessingOptions SetMaxAge(TimeSpan maxAge)
    {
        MaxAge = maxAge;
        return this;
    }

    public AsyncCommandProcessingOptions SetMessageTimeout(TimeSpan messageTimeout)
    {
        MessageTimeout = messageTimeout;
        return this;
    }

    public AsyncCommandProcessingOptions SetMaxRetryCount(int maxRetryCount)
    {
        if (maxRetryCount < 0)
            throw new ArgumentException(nameof(maxRetryCount));

        MaxRetryCount = maxRetryCount;
        return this;
    }

    public AsyncCommandProcessingOptions SetMaxDegreeOfParallelism(int maxDegreeOfParallelism)
    {
        if (maxDegreeOfParallelism < 0)
            throw new ArgumentException(nameof(maxDegreeOfParallelism));

        MaxDegreeOfParallelism = maxDegreeOfParallelism;
        return this;
    }

    public AsyncCommandProcessingOptions UseRoundRobinConsumerStrategy()
    {
        ConsumerStrategy = SystemConsumerStrategies.RoundRobin;
        return this;
    }

    public AsyncCommandProcessingOptions UsePinnedConsumerStrategy()
    {
        ConsumerStrategy = SystemConsumerStrategies.Pinned;
        return this;
    }

    public AsyncCommandProcessingOptions UseDispatchToSingleConsumerStrategy()
    {
        ConsumerStrategy = SystemConsumerStrategies.DispatchToSingle;
        return this;
    }

}

public class AsyncEventProcessingOptions
{
    public TimeSpan MessageTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetryCount { get; set; } = 10;
    public string ConsumerStrategy { get; set; } = SystemConsumerStrategies.RoundRobin;
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount / 2;

    public AsyncEventProcessingOptions SetMessageTimeout(TimeSpan messageTimeout)
    {
        MessageTimeout = messageTimeout;
        return this;
    }

    public AsyncEventProcessingOptions SetMaxRetryCount(int maxRetryCount)
    {
        if (maxRetryCount < 0)
            throw new ArgumentException(nameof(maxRetryCount));

        MaxRetryCount = maxRetryCount;
        return this;
    }

    public AsyncEventProcessingOptions SetMaxDegreeOfParallelism(int maxDegreeOfParallelism)
    {
        if (maxDegreeOfParallelism < 0)
            throw new ArgumentException(nameof(maxDegreeOfParallelism));

        MaxDegreeOfParallelism = maxDegreeOfParallelism;
        return this;
    }

    public AsyncEventProcessingOptions UseRoundRobinConsumerStrategy()
    {
        ConsumerStrategy = SystemConsumerStrategies.RoundRobin;
        return this;
    }

    public AsyncEventProcessingOptions UsePinnedConsumerStrategy()
    {
        ConsumerStrategy = SystemConsumerStrategies.Pinned;
        return this;
    }

    public AsyncEventProcessingOptions UseDispatchToSingleConsumerStrategy()
    {
        ConsumerStrategy = SystemConsumerStrategies.DispatchToSingle;
        return this;
    }
}