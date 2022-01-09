namespace EventStore.Library.Core;

internal class AggregateRootIdNotSetException : InvalidOperationException
{
    public AggregateRootIdNotSetException(string aggregateRootType) : base($"Id was not set for aggregate root: {aggregateRootType}, Make sure creation event sets aggregate root id")
    {
    }
}