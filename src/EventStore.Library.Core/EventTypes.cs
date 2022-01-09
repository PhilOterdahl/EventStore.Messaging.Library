using System.Reflection;
using EventStore.Library.Core.Domain.Aggregate;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Core;

public static class EventTypes
{
    private static IDictionary<string, Type> Types { get; set; } = new Dictionary<string, Type>(new Dictionary<string, Type>());

    public static IEnumerable<Type> GetTypes(Type baseEventType) => Types.Values.Where(type => type.IsAssignableFrom(baseEventType));

    public static Type GetEventType(string eventName) => Types[eventName];

    public static Type? TryGetEventType(string eventName) =>
        Types.TryGetValue(eventName, out var eventType) 
            ? eventType 
            : null;

    public static void SetEventTypes(Assembly[] assemblies)
    {
        Types = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract)
            .Where(type => !type.IsGenericType)
            .Where(type => type.IsAssignableTo(typeof(IEventStoreEvent)))
            .ToDictionary(type =>
            {
                var version = type.GetCustomAttribute<EventAttribute>();
                var key = version is not null 
                    ? $"{type.Name}-{version.Version}" 
                    : type.Name;

                return key;
            });
    }

    public static void AddEventTypes(params Assembly[] assemblies)
    {
        var types = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract)
            .Where(type => !type.IsGenericType)
            .Where(type => type.IsAssignableTo(typeof(IEventStoreEvent)));

        var duplicateEventType = Types.FirstOrDefault(type => types.Contains(type.Value)).Value;

        if (duplicateEventType is not null)
            throw new InvalidOperationException($"Can not add eventType, eventType: {duplicateEventType.Name} is already added");

        foreach (var type in types)
        {
            Types[type.Name] = type;
        }
    }
}