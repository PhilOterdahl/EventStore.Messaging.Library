using EventStore.Library.Core;
using EventStore.Library.Examples.Messaging.Aggregates.Events;
using EventStore.Library.Examples.Messaging.DataExtensions;
using EventStore.Library.Messaging.Event;

namespace EventStore.Library.Examples.Messaging.Processes;

internal class SendWelcomeLetterEventHandler : IAsyncEventHandler<UserEnrolledEvent>
{
    private readonly ILogger<SendWelcomeLetterEventHandler> _logger;
    private readonly IEventStore _eventStore;

    public SendWelcomeLetterEventHandler(ILogger<SendWelcomeLetterEventHandler> logger, IEventStore eventStore)
    {
        _logger = logger;
        _eventStore = eventStore;
    }

    public async Task Handle(UserEnrolledEvent @event, CancellationToken cancellationToken)
    {
        var (userId, firstName, lastName, _, by) = @event;
        var user = await _eventStore.Load(userId, cancellationToken);
        user.WelcomeLetterSent(by);

        await _eventStore.CommitEvents(user, cancellationToken);

        _logger.LogInformation($"Welcome letter sent to: {firstName} {lastName}, by: {by}");
    }
}