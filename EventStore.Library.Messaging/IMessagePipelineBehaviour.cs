namespace EventStore.Library.Messaging;

/// <summary>
/// Pipeline behavior to surround the inner handler.
/// Implementations add additional behavior
/// </summary>
/// <typeparam name="TMessage">Message type</typeparam>
public interface IMessagePipelineBehavior<TMessage>
    where TMessage : IMessage
{
    /// <summary>
    /// Pipeline handler. Perform any additional behavior
    /// </summary>
    /// <param name="message">Incoming Message</param>
    /// <param name="next">Awaitable delegate for the next action in the pipeline. Eventually this delegate represents the handler if it is not an async message.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task returning the <typeparamref name="TMessage"/></returns>
    Task<TMessage> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage> next,
        CancellationToken cancellationToken);
}

/// <summary>
/// Represents an async continuation for the next task to execute in the pipeline
/// </summary>
/// <typeparam name="TMessage">Response type</typeparam>
/// <returns>Awaitable task returning a <typeparamref name="TMessage" /></returns>
public delegate Task<TMessage> MessageHandlerDelegate<TMessage>() where TMessage : IMessage;