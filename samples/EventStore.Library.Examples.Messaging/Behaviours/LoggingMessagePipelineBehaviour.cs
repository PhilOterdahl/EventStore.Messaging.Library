using System.Text.Json;
using EventStore.Library.Messaging;

namespace EventStore.Library.Examples.Messaging.Behaviours;

internal class LoggingMessagePipelineBehaviour<TMessage> : IMessagePipelineBehavior<TMessage> where TMessage : IMessage
{
    private readonly ILogger _logger;

    public LoggingMessagePipelineBehaviour(
        ILogger<IMessagePipelineBehavior<TMessage>> logger)
    {
        _logger = logger;
    }

    public async Task<TMessage> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage> next,
        CancellationToken cancellationToken)
    {
        var messageName = message.GetType().Name;
        var messageBody = GetMessageBody(message);
        var messageType = GetMessageType(messageName);

        _logger.LogInformation(
            $"Message sent, Message type: {messageType}{Environment.NewLine}" +
            $"Message name: {messageName}{Environment.NewLine}" +
            $"Body: {messageBody}");

        try
        {
            return await next();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"{messageName} failed");
            throw;
        }
    }

    private static string GetMessageType(string messageName)
    {
        if (messageName.Contains("Command"))
            return "Command";

        if (messageName.Contains("Query"))
            return "Query";

        if (messageName.Contains("Event"))
            return "Event";

        return "Unknown";
    }

    private static string GetMessageBody<T>(T message) =>
        JsonSerializer.Serialize(message);
}
