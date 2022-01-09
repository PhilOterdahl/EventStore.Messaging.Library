using EventStore.Library.Core;
using EventStore.Library.Examples.Messaging.Aggregates;
using EventStore.Library.Examples.Messaging.DataExtensions;
using EventStore.Library.Messaging.Command;

namespace EventStore.Library.Examples.Messaging.Processes;

internal class EnrollUserCommand : AsyncCommand
{
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }

    public EnrollUserCommand(
        string firstName,
        string lastName,
        string email,
        string @by) : base(@by)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public void Deconstruct(
        out string firstName,
        out string lastName,
        out string email, 
        out string by)
    {
        firstName = FirstName;
        lastName = LastName;
        email = Email;
        by = By;
    }
}

internal class EnrollUserCommandHandler : IAsyncCommandHandler<EnrollUserCommand>
{
    private readonly ILogger<EnrollUserCommandHandler> _logger;
    private readonly IEventStore _eventStore;

    public EnrollUserCommandHandler(ILogger<EnrollUserCommandHandler> logger, IEventStore eventStore)
    {
        _logger = logger;
        _eventStore = eventStore;
    }

    public async Task Handle(EnrollUserCommand command, CancellationToken cancellationToken)
    {
        var (firstName, lastName, email, by) = command;
        var user = User.Enroll(firstName, lastName, email, by);
        await _eventStore.CommitEvents(user, cancellationToken);

        _logger.LogInformation($"User: {firstName} {lastName} enrolled by {by}");
    }
}
