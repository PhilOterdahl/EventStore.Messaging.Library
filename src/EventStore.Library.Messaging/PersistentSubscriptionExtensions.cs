using EventStore.Client;
using EventStore.Library.Core.Event;
using Microsoft.Extensions.Logging;

namespace EventStore.Library.Messaging;

public static class PersistentSubscriptionExtensions
{
    public static async Task RetryMessage(
        this PersistentSubscription subscription,
        ResolvedEvent resolvedEvent,
        IEventStoreEvent baseEvent,
        Exception exception,
        ILogger logger)
    {
        await subscription.Nack(
            PersistentSubscriptionNakEventAction.Retry,
            $"Unexpected error, retrying message of type: {baseEvent.GetEventType()} with Id: {baseEvent.Id}, Error: {exception.Message}",
            resolvedEvent);
        logger.LogError($"Unexpected error for message bus, retrying message of type: : {baseEvent.GetEventType()} with Id: {baseEvent.Id}");
    }

    public static async Task ParkMessage(
        this PersistentSubscription subscription,
        ResolvedEvent resolvedEvent,
        IEventStoreEvent baseEvent,
        Exception exception,
        ILogger logger)
    {
        await subscription.Nack(
            PersistentSubscriptionNakEventAction.Park,
            $"Unexpected error, parking message of type: {baseEvent.GetEventType()} with Id: {baseEvent.Id}, Error: {exception.Message}",
            resolvedEvent);

        logger.LogError($"Unexpected error for message bus, parking message of type: : {baseEvent.GetEventType()} with Id: {baseEvent.Id}");
    }
}