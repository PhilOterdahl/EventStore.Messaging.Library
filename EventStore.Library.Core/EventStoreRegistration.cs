using System.Reflection;
using EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventStore.Library.Core;

public static class EventStoreRegistration
{
    public static IServiceCollection AddEventStore(this IServiceCollection services,
        EventStoreClientOptions eventStoreClientOptions,
        Action<EventStoreOptions>? configureOptions,
        params Assembly[] assemblies)
    {
        var options = new EventStoreOptions(eventStoreClientOptions);
        configureOptions?.Invoke(options);

        if (assemblies is null)
            throw new ArgumentNullException(nameof(assemblies));

        if (!assemblies.Any())
            throw new ArgumentException("assemblies can not be empty");

        EventTypes.SetEventTypes(assemblies);

        return services
            .AddSingleton(options)
            .AddSingleton<IEventStore, EventStore>()
            .AddSingleton(serviceProvider =>
            {
                var settings = GetEventStoreClientSettings(serviceProvider);
                return new EventStoreClient(settings);
            })
            .AddSingleton(serviceProvider =>
            {
                var settings = GetEventStoreClientSettings(serviceProvider);
                return new EventStoreProjectionManagementClient(settings);
            })
            .AddSingleton(serviceProvider =>
            {
                var settings = GetEventStoreClientSettings(serviceProvider);
                return new EventStorePersistentSubscriptionsClient(settings);
            });
    }

    private static EventStoreClientSettings GetEventStoreClientSettings(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<EventStoreOptions>();
        var clientOptions = options.ClientOptions;
        var operationOptions = options.ClientOperationOptions;
        var settings = EventStoreClientSettings.Create(clientOptions.ConnectionStringGrcp);
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        settings.OperationOptions = operationOptions;

        if (!settings.ConnectivitySettings.Insecure)
        {
            settings.DefaultCredentials = new UserCredentials(clientOptions.Username, clientOptions.Password);
        }
        else
        {
#pragma warning disable S4830
            // Enable support for unencrypted HTTP2  
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var clientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            settings.CreateHttpMessageHandler = () => clientHandler;
            settings.ChannelCredentials = ChannelCredentials.Insecure;
#pragma warning restore S4830
        }

        settings.LoggerFactory = loggerFactory;
        return settings;
    }
}