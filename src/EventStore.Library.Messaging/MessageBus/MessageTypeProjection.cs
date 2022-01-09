namespace EventStore.Library.Messaging.MessageBus;

internal static class MessageTypeProjection
{
    public const string StreamName = "$message-type";

    public const string Projection = @"
            fromAll()
            .when({
                $any: function(state, event){
                    if (event.metadata !== null){
                         if (event.metadata.MessageType == 'async-command')
                            linkTo('async-command', event, {});
                        else if (event.metadata.MessageType == 'async-event')
                            linkTo('async-event', event, {});
                    }
                }
            })";
}