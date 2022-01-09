namespace EventStore.Library.Messaging;

internal class NoCommandHandlerRegisteredException : InvalidOperationException
{
    public NoCommandHandlerRegisteredException(string commandType) : base($"No command handler registered for command: {commandType}")
    {
            
    }
}