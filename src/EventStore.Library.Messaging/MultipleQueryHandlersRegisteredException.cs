namespace EventStore.Library.Messaging;

internal class MultipleQueryHandlersRegisteredException : InvalidOperationException
{
    public MultipleQueryHandlersRegisteredException(string queryType) : base($"Multiple query handlers registered for query: {queryType}")
    {
    }
}