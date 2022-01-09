using EventStore.Client;

namespace EventStore.Library.Core.Domain.Aggregate;

public class InvalidEventByMissingException : InvalidOperationException
{
    public InvalidEventByMissingException(Uuid id, string eventType) : base($"Invalid event 'by' is not set, Id: {id}, type: {eventType}")
    {
    }
}