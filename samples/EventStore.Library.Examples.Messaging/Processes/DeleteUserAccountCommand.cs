using EventStore.Library.Core;
using EventStore.Library.Examples.Messaging.Aggregates;
using EventStore.Library.Examples.Messaging.DataExtensions;
using EventStore.Library.Messaging.Command;

namespace EventStore.Library.Examples.Messaging.Processes;

internal class DeleteUserAccountCommand : ICommand<bool>
{
    public UserId UserId { get; }
    public string By { get; }

    public DeleteUserAccountCommand(UserId userId, string @by)
    {
        UserId = userId;
        By = @by;
    }

    public void Deconstruct(out UserId userId, out string by)
    {
        userId = UserId;
        by = By;
    }
}

internal class DeleteUserAccountCommandHandler : ICommandHandler<DeleteUserAccountCommand, bool>
{
    private readonly ILogger _logger;
    private readonly IEventStore _eventStore;

    public DeleteUserAccountCommandHandler(ILogger<DeleteUserAccountCommandHandler> logger, IEventStore eventStore)
    {
        _logger = logger;
        _eventStore = eventStore;
    }

    public async Task<bool> Handle(DeleteUserAccountCommand command, CancellationToken cancellationToken)
    {
        var (userId, by) = command;
        var user = await _eventStore.TryLoad(userId, cancellationToken);

        if (user == null)
        {
            _logger.LogError($"Delete user account failed, User account: {userId.Value} could not be found");
            return false;
        }

        user.Delete(by);

        await _eventStore.CommitEvents(user, cancellationToken);
        _logger.LogInformation($"User account: {userId.Value} deleted");
        return true;
    }
}