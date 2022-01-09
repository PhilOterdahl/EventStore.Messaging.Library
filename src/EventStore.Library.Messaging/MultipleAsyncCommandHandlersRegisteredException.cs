namespace EventStore.Library.Messaging;

internal class MultipleAsyncCommandHandlersRegisteredException : InvalidOperationException
{
    public MultipleAsyncCommandHandlersRegisteredException(string asyncCommandType) : base($"Multiple async command handlers registered for async command: {asyncCommandType}")
    {
    }
}