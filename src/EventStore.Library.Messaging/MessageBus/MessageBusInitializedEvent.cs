using System.ComponentModel;
using System.Text.Json.Serialization;
using EventStore.Library.Messaging.Event;

namespace EventStore.Library.Messaging.MessageBus;

[Description("Published when event store messageBus has been intialized")]
public class MessageBusInitializedEvent : AsyncEvent
{
    public MessageBusInitializedEvent() : base("MessageBus")
    {
    }
}