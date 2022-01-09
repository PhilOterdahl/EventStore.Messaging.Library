namespace EventStore.Library.Messaging;

internal class NoAsyncCommandHandlerRegisteredException : InvalidOperationException
{
    public NoAsyncCommandHandlerRegisteredException(string asyncCommandType) : base($"No async command handler is registered for async command: {asyncCommandType}")
    {
            
    }
}