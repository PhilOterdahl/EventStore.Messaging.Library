namespace EventStore.Library.Messaging.MessageBus;

public enum Status
{
    Processing,
    Processed,
    Retrying,
    Parked
}

public class MessageStatus
{
    public Guid Id { get; set; }
    public string Handler { get; set; }
    public string Stream { get; set; }
    public long Number { get; set; }
    public Status Status { get; set; }
}