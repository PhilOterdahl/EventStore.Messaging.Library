namespace EventStore.Library.Core;

public class AggregateRootAlreadyCreatedException : InvalidOperationException
{
    public AggregateRootAlreadyCreatedException(string aggregateRootType) : base(
        $"Aggregate root: {aggregateRootType}, already contains event and can not be created")

    {
    }
}