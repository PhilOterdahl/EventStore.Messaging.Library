using System.Reflection;
using EventStore.Library.Messaging.Command;
using EventStore.Library.Messaging.Event;
using EventStore.Library.Messaging.MessageBus;
using EventStore.Library.Messaging.Query;
using Microsoft.Extensions.DependencyInjection;

namespace EventStore.Library.Messaging.Registration;

public static class EventStoreMessagingRegistrationExtensions
{
    public static IServiceCollection AddEventStoreMessaging(
        this IServiceCollection services,
        Action<MessageBusOptions>? configureOptions,
        params Assembly[] assemblies)
    {
        var options = new MessageBusOptions();
        configureOptions?.Invoke(options);

        services
            .AddSingleton(options)
            .AddCommandHandlers(assemblies)
            .AddQueryHandlers(assemblies)
            .AddEventHandlers(assemblies)
            .AddBehaviors(assemblies);

        return options.MessageStatusRepositoryType switch
        {
            MessageStatusRepositoryType.InMemory => services
                .AddScoped<IMessageStatusRepository, InMemoryMessageStatusRepository>(),
            MessageStatusRepositoryType.SqlServer => services
                .AddScoped<IMessageStatusRepository, InMemoryMessageStatusRepository>(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services, Assembly[] assemblies) =>
        services
            .AddAllImplementingInterface(assemblies, typeof(IAsyncCommandHandler<>))
            .AddAllImplementingInterface(assemblies, typeof(ICommandHandler<,>));

    private static IServiceCollection AddEventHandlers(this IServiceCollection services, Assembly[] assemblies) =>
        services
            .AddAllImplementingInterface(assemblies, typeof(IAsyncEventHandler<>));

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services, Assembly[] assemblies) =>
        services
            .AddAllImplementingInterface(assemblies, typeof(IQueryHandler<,>));

    private static IServiceCollection AddBehaviors(this IServiceCollection services, Assembly[] assemblies) =>
        services
            .AddAllImplementingInterface(assemblies, typeof(IMessagePipelineBehavior<>))
            .AddAllImplementingInterface(assemblies, typeof(IQueryPipelineBehavior<,>));

    private static IServiceCollection AddAllImplementingInterface(
        this IServiceCollection services,
        Assembly[] assemblies,
        Type interfaceType,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        var types = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(implementationType => !implementationType.IsAbstract && !implementationType.IsGenericTypeDefinition)
            .WhereImplementingInterface(interfaceType);

        foreach (var handlerType in types)
        {
            var implementedInterfaceTypes = handlerType
                .GetInterfaces()
                .Where(implementedInterfaceType => implementedInterfaceType.IsGenericType && implementedInterfaceType.GetGenericTypeDefinition() == interfaceType ||
                                                   implementedInterfaceType == interfaceType);

            foreach (var implementedInterfaceType in implementedInterfaceTypes)
            {
                services.Add(new ServiceDescriptor(implementedInterfaceType, handlerType, serviceLifetime));
            }
        }

        return services;
    }

    private static IEnumerable<Type> WhereImplementingInterface(this IEnumerable<Type> types, params Type[] interfaceTypes) =>
        types.Where(type => type
            .GetInterfaces()
            .Any(interfaceType => interfaceType.IsGenericType &&
                                  interfaceTypes.Contains(interfaceType.GetGenericTypeDefinition())
            )
        );
}