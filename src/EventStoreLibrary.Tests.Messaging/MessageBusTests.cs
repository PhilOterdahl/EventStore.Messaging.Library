using Xunit;

namespace EventStoreLibrary.Tests.Messaging;

[Collection("Event store")]
public class MessageBusTests
{
    private readonly EventStoreFixture _eventStoreFixture;

    public MessageBusTests(EventStoreFixture eventStoreFixture)
    {
        _eventStoreFixture = eventStoreFixture;
    }

    public void Test1()
    {

    }
}