using EventStore.Client;

namespace EventStore.Library.Core.Event;

public interface IEventStoreEvent
{
    public Uuid Id { get; set; }
    public DateTime Date { get; set; }
    public string By { get; set; }

    public byte[] ToData();
    public string GetEventType();
    public EventData ToDataModel();
    public EventMetaData ToMetaData();
}