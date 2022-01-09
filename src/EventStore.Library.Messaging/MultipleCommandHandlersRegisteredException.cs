namespace EventStore.Library.Messaging;

internal class MultipleCommandHandlersRegisteredException : InvalidOperationException
{
    public MultipleCommandHandlersRegisteredException(string commandType) : base($"Multiple command handlers registered for command: {commandType}")
    {
    }
}