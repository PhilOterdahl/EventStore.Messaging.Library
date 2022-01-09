namespace EventStore.Library.Core.Domain.Aggregate;

internal class DomainEventNotHandledException : InvalidOperationException
{
    public DomainEventNotHandledException(string eventType)
        : base($"Event: {eventType} is not handled, either add the NotModifyAggregateStateAttribute or call When method for specific event")
    {
    }
}