namespace EventStore.Library.Messaging.Command;

/// <summary>Defines a handler for a command</summary>
/// <typeparam name="TCommand">The type of command being handled</typeparam>
public interface IAsyncCommandHandler<in TCommand> where TCommand : IAsyncCommand
{
    /// <summary>Handles an async command</summary>
    /// <param name="command">The command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Handle(TCommand command, CancellationToken cancellationToken);
}