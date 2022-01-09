using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Headers;
using EventStore.Client;
using EventStore.Library.Core;
using EventStore.Library.Core.Event;
using EventStore.Library.Messaging.Command;
using EventStore.Library.Messaging.Event;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventStore.Library.Messaging.MessageBus;

internal class MessageBus : BackgroundService, IMessageBus
{
    private const string GroupName = "messagebus";
    private const string MessageTypeProjectionName = "message-type";

    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly EventStoreOptions _eventStoreOptions;
    private readonly MessageBusOptions _messageBusOptions;

    private readonly IDictionary<string, PersistentSubscription> _subscriptions = new Dictionary<string, PersistentSubscription>();

    public MessageBus(
        ILogger<MessageBus> logger,
        IServiceProvider serviceProvider,
        MessageBusOptions messageBusOptions, 
        EventStoreOptions eventStoreOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _messageBusOptions = messageBusOptions;
        _eventStoreOptions = eventStoreOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var projectionManagementClient = scope.ServiceProvider.GetRequiredService<EventStoreProjectionManagementClient>();
        var eventStoreClient = scope.ServiceProvider.GetRequiredService<EventStoreClient>();
        var persistentSubscriptionClient = scope.ServiceProvider.GetRequiredService<EventStorePersistentSubscriptionsClient>();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();

        await CreateRequiredProjections(projectionManagementClient, stoppingToken);
        await SetAsyncCommandMaxAge(eventStoreClient, _messageBusOptions.AsyncCommandProcessingOptions.MaxAge, stoppingToken);
        await CreatePersistentSubscriptions(environment, persistentSubscriptionClient, stoppingToken);
        await SubscribeToEvents(persistentSubscriptionClient, stoppingToken);
        await PublishMessageBusInitializedEvent(dispatcher, stoppingToken);
    }

    private static async Task PublishMessageBusInitializedEvent(IDispatcher dispatcher, CancellationToken stoppingToken) => 
        await dispatcher.Publish(new MessageBusInitializedEvent(), stoppingToken);

    private async Task CreatePersistentSubscriptions(
        IHostEnvironment environment, 
        EventStorePersistentSubscriptionsClient client,
        CancellationToken stoppingToken)
    {
        if (!await SubscriptionExists(environment, MessageStreams.AsyncCommand, GroupName))
        {
            await client.CreateAsync(MessageStreams.AsyncCommand, GroupName, new PersistentSubscriptionSettings(
                true,
                StreamPosition.End,
                namedConsumerStrategy: _messageBusOptions.AsyncEventProcessingOptions.ConsumerStrategy,
                maxRetryCount: _messageBusOptions.AsyncCommandProcessingOptions.MaxRetryCount,
                messageTimeout: _messageBusOptions.AsyncCommandProcessingOptions.MessageTimeout
            ), cancellationToken: stoppingToken);

            _logger.LogInformation($"Persistent subscription created, stream: {MessageStreams.AsyncCommand}, group: {GroupName}");
        }

        if (!await SubscriptionExists(environment, MessageStreams.AsyncEvent, GroupName))
        {
            await client.CreateAsync(MessageStreams.AsyncEvent, GroupName, new PersistentSubscriptionSettings(
                true,
                StreamPosition.End,
                namedConsumerStrategy: _messageBusOptions.AsyncEventProcessingOptions.ConsumerStrategy,
                maxRetryCount: _messageBusOptions.AsyncEventProcessingOptions.MaxRetryCount,
                messageTimeout: _messageBusOptions.AsyncEventProcessingOptions.MessageTimeout
            ), cancellationToken: stoppingToken);

            _logger.LogInformation($"Persistent subscription created, stream: {MessageStreams.AsyncEvent}, group: {GroupName}");
        }
    }

    private async Task SubscribeToEvents(EventStorePersistentSubscriptionsClient client, CancellationToken stoppingToken)
    {
        await Subscribe(
            client, 
            MessageStreams.AsyncEvent, 
            GroupName,
            EventAppeared,
            _messageBusOptions.AsyncEventProcessingOptions.MaxDegreeOfParallelism,
            autoAck: false,
            cancellationToken: stoppingToken);

        await Subscribe(
            client, 
            MessageStreams.AsyncCommand,
            GroupName,
            CommandAppeared, 
            _messageBusOptions.AsyncCommandProcessingOptions.MaxDegreeOfParallelism,
            autoAck: false, 
            cancellationToken: stoppingToken);
    }

    private async Task Subscribe(
        EventStorePersistentSubscriptionsClient client,
        string stream,
        string group,
        Func<PersistentSubscription, ResolvedEvent, int?, CancellationToken, Task> eventAppeared,
        int maxDegreeOfParallelism = 1,
        int bufferSize = 10,
        bool autoAck = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Subscribing to persistent subscription for stream: {stream}, group: {group} in {maxDegreeOfParallelism} threads");

        for (var index = 0; index < maxDegreeOfParallelism; index++)
        {
            var subscription = await client.SubscribeAsync(
                stream,
                group,
                eventAppeared,
                SubscriptionDropped(stream, @group, autoAck, eventAppeared, cancellationToken)!,
                bufferSize: bufferSize,
                autoAck: autoAck,
                cancellationToken: cancellationToken);

            _subscriptions.TryAdd(subscription.SubscriptionId, subscription);

            _logger.LogInformation($"Subscribed to stream: {stream}, group: {group}, subscriptionId: {subscription.SubscriptionId}");
        }
    }

    private async Task EventAppeared(
        PersistentSubscription subscription,
        ResolvedEvent @event,
        int? retryCount,
        CancellationToken cancellationToken)
    {
        var asyncEvent = @event.ToAsyncEvent();
        await using var scope = _serviceProvider.CreateAsyncScope();

        await Process(
            scope,
            @event,
            asyncEvent, () => HandleAsyncEvent(asyncEvent, scope, cancellationToken),
            subscription,
            retryCount ?? 0,
            cancellationToken);
    }

    private async Task CommandAppeared(
        PersistentSubscription subscription,
        ResolvedEvent @event,
        int? retryCount,
        CancellationToken cancellationToken)
    {
        var asyncCommand = @event.ToAsyncCommand();
        await using var scope = _serviceProvider.CreateAsyncScope();

        await Process(
            scope,
            @event,
            asyncCommand, () => HandleAsyncCommand(asyncCommand, scope, cancellationToken),
            subscription,
            retryCount ?? 0,
            cancellationToken);
    }

    public async Task Process(
        IServiceScope scope,
        ResolvedEvent resolvedEvent,
        IEventStoreEvent @event,
        Func<Task> getHandleTask,
        PersistentSubscription subscription,
        int retryCount,
        CancellationToken cancellationToken)
    {
        var handler = GetType().Name;
        var metadata = resolvedEvent.ToEventMetaData();

        if (!metadata.ShouldProcess)
            return;

        var messageStatusRepository = GetMessageStatusRepository(scope);

        var messageStatus = await TryLoadMessageStatus(messageStatusRepository, resolvedEvent, @event, handler, cancellationToken);

        if (messageStatus?.Status is Status.Processing or Status.Processed)
            return;

        var incorrectOrder = IsNotInOrder(resolvedEvent, messageStatus);

        if (incorrectOrder)
            await RetryIncorrectOrderEvent(resolvedEvent, subscription);

        messageStatus ??= await ProcessingMessage(messageStatusRepository, resolvedEvent, @event, handler, cancellationToken);

        //If messageStatus is null, processing failed and messages status for message has already been saved by a process in another thread
        if (messageStatus is null)
            return;

        try
        {
            await getHandleTask();
            await SetMessageProcessed(resolvedEvent, messageStatus, subscription, messageStatusRepository, cancellationToken);
        }
        catch (Exception exception)
        {
            var maxRetryCount = @event is IAsyncCommand
                ? _messageBusOptions.AsyncCommandProcessingOptions.MaxRetryCount
                : _messageBusOptions.AsyncEventProcessingOptions.MaxRetryCount;

            var shouldPark = retryCount > maxRetryCount;

            var actionTask = shouldPark
                ? ParkMessage(resolvedEvent, @event, messageStatus, subscription, messageStatusRepository, exception, cancellationToken)
                : RetryMessage(resolvedEvent, @event, messageStatus, subscription, messageStatusRepository, exception, cancellationToken);

            await actionTask;
        }
    }

    private static IMessageStatusRepository GetMessageStatusRepository(IServiceScope scope) => scope.ServiceProvider.GetRequiredService<IMessageStatusRepository>();

    private static bool IsNotInOrder(ResolvedEvent resolvedEvent, MessageStatus? messageStatus) => messageStatus is not null && messageStatus.Number + 1 != resolvedEvent.Event.EventNumber.ToInt64();

    private static async Task RetryIncorrectOrderEvent(ResolvedEvent resolvedEvent, PersistentSubscription subscription) => 
        await subscription.Nack(PersistentSubscriptionNakEventAction.Retry, "Incorrect order", resolvedEvent);

    private static async Task CreateRequiredProjections(EventStoreProjectionManagementClient projectionManagementClient, CancellationToken stoppingToken)
    {
        var result = await projectionManagementClient
            .ListContinuousAsync(cancellationToken: stoppingToken)
            .ToArrayAsync(stoppingToken);

        if (!result.WhereProjection(SystemProjections.ByCategory).WhereRunning().Any())
            await projectionManagementClient.EnableAsync(SystemProjections.ByCategory, cancellationToken: stoppingToken);

        if (!result.WhereProjection(MessageTypeProjectionName).Any())
            await projectionManagementClient.CreateContinuousAsync(MessageTypeProjectionName, MessageTypeProjection.Projection, true, cancellationToken: stoppingToken);
    }

    private static async Task SetAsyncCommandMaxAge(EventStoreClient eventStoreClient, TimeSpan maxAge, CancellationToken stoppingToken) =>
        await eventStoreClient.SetStreamMetadataAsync(
            MessageStreams.AsyncCommand,
            StreamState.Any,
            new StreamMetadata(maxAge: maxAge),
            cancellationToken: stoppingToken);

    private async Task<bool> SubscriptionExists(IHostEnvironment environment, string stream, string group)
    {
        using var client = CreateHttpClient(environment);

        using var response = await client.GetAsync($"{_eventStoreOptions.ClientOptions.ConnectionStringHttp}/subscriptions/{stream}/{group}/info");
        return response.StatusCode switch
        {
            HttpStatusCode.OK => true,
            _ => false
        };
    }

    private Action<PersistentSubscription, SubscriptionDroppedReason, Exception> SubscriptionDropped(
        string stream,
        string group,
        bool autoAck,
        Func<PersistentSubscription, ResolvedEvent, int?, CancellationToken, Task> eventAppeared,
        CancellationToken stoppingToken)
        => (persistentSubscription, subscriptionDropReason, exception) =>
        {
            Error(subscriptionDropReason, exception, stream, group);
            persistentSubscription.Dispose();
            _subscriptions.Remove(persistentSubscription.SubscriptionId);
            using var scope = _serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<EventStorePersistentSubscriptionsClient>();
            Subscribe(client, stream, group, eventAppeared, 1, autoAck: autoAck, cancellationToken: stoppingToken).Wait(stoppingToken);
        };

    private void Error(
        SubscriptionDroppedReason subscriptionDropReason,
        Exception exception,
        string stream,
        string group)
    {
        switch (subscriptionDropReason)
        {
            case SubscriptionDroppedReason.SubscriberError:
                _logger.LogError($"Stream: {stream}, group: {group} persistent subscription dropped by error in subscriber. error: {exception.Message}");
                break;
            case SubscriptionDroppedReason.ServerError:
                _logger.LogError($"Stream: {stream}, group: {group} persistent subscription stopped because of a server error ({subscriptionDropReason}). ", exception);
                _logger.LogInformation("Attempting to restart...");
                break;
            case SubscriptionDroppedReason.Disposed:
                _logger.LogError($"Stream: {stream}, group: {group} persistent subscription dropped subscription was disposed.");
                break;
            default:
                _logger.LogError($"Stream: {stream}, group: {group} persistent subscription dropped unknown error.", exception);
                break;
        }
    }

    private HttpClient CreateHttpClient(IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
#pragma warning disable S4830
            return new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            });
#pragma warning restore S4830
        }

        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{_eventStoreOptions.ClientOptions.Username}:{_eventStoreOptions.ClientOptions.Password}");
        var encoded = Convert.ToBase64String(plainTextBytes);

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationSchemes.Basic.ToString(), encoded);

        return client;
    }

    private static async Task<MessageStatus?> ProcessingMessage(
        IMessageStatusRepository repository,
        ResolvedEvent resolvedEvent,
        IEventStoreEvent baseEvent,
        string handler,
        CancellationToken cancellationToken) =>
        await repository.ProcessingMessage(
            baseEvent.Id.ToGuid(), 
            handler, 
            resolvedEvent.Event.EventStreamId,
            resolvedEvent.Event.EventNumber.ToInt64(), 
            cancellationToken);
    
    private static async Task<MessageStatus?> TryLoadMessageStatus(
        IMessageStatusRepository repository,
        ResolvedEvent resolvedEvent,
        IEventStoreEvent baseEvent,
        string handler,
        CancellationToken cancellationToken) =>
        await repository.TryLoadMessageStatus(
            baseEvent,
            handler,
            resolvedEvent.Event.EventStreamId,
            cancellationToken);

    private static async Task SetMessageProcessed(
        ResolvedEvent resolvedEvent,
        MessageStatus messageStatus,
        PersistentSubscription subscription,
        IMessageStatusRepository repository,
        CancellationToken cancellationToken)
    {
        await repository.SetMessageProcessed(messageStatus, cancellationToken);
        await subscription.Ack(resolvedEvent);
    }

    private async Task RetryMessage(
        ResolvedEvent resolvedEvent,
        IEventStoreEvent baseEvent,
        MessageStatus messageStatus,
        PersistentSubscription subscription,
        IMessageStatusRepository repository,
        Exception exception,
        CancellationToken cancellationToken)
    {
        await repository.RetryMessage(messageStatus, cancellationToken);
        await subscription.RetryMessage(resolvedEvent, baseEvent, exception, _logger);
    }

    private async Task ParkMessage(
        ResolvedEvent resolvedEvent,
        IEventStoreEvent baseEvent,
        MessageStatus messageStatus,
        PersistentSubscription subscription,
        IMessageStatusRepository repository,
        Exception exception,
        CancellationToken cancellationToken)
    {
        await repository.ParkMessage(messageStatus, cancellationToken);
        await subscription.ParkMessage(resolvedEvent, baseEvent, exception, _logger);
    }

    private static async Task HandleAsyncEvent(
        IAsyncEvent asyncEvent,
        IServiceScope scope,
        CancellationToken cancellationToken)
    {
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await sender.Send(asyncEvent, cancellationToken);
    }

    private static async Task HandleAsyncCommand(
        IAsyncCommand asyncCommand,
        IServiceScope scope,
        CancellationToken cancellationToken)
    {
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await sender.Send(asyncCommand, cancellationToken);
    }

    public override void Dispose()
    {
        foreach (var subscription in _subscriptions.Values)
        {
            subscription.Dispose();
        }

        _logger.LogWarning($"EventStore messageBus disposed");
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}