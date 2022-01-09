namespace EventStore.Library.Messaging;

internal class NoQueryHandlerRegisteredException : InvalidOperationException
{
    public NoQueryHandlerRegisteredException(string queryType) : base($"No query handler registered for query: {queryType}")
    {
            
    }
}