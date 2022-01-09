using System.Reflection;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Core;

public static class EventTypes
{
    private static IDictionary<string, Type> Types { get; set; } = new Dictionary<string, Type>(new Dictionary<string, Type>());

    public static IEnumerable<Type> GetTypes(Type baseEventType) => Types.Values.Where(type => type.IsAssignableTo(baseEventType));

    public static Type GetEventType(string eventName) => Types[eventName];

    public static Type? TryGetEventType(string eventName) =>
        Types.TryGetValue(eventName, out var eventType) 
            ? eventType 
            : null;

    public static void SetEventTypes(Assembly[] assemblies)
    {
        var eventTypeInformation = GetEventTypeInformation(assemblies);

        Types = eventTypeInformation.ToDictionary(eventInformation => eventInformation.Item1, eventInformation => eventInformation.Item2);
    }

    private static IEnumerable<Tuple<string, Type>> GetEventTypeInformation(Assembly[] assemblies)
    {
        var eventTypeInformation = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract)
            .Where(type => !type.IsGenericType)
            .Where(type => type.IsAssignableTo(typeof(IEventStoreEvent)))
            .Select(type => Tuple.Create(type.GetEventType(), type ))
            .ToArray();

        var duplicateEvent = eventTypeInformation
            .GroupBy(information => information.Item1)
            .FirstOrDefault(group => @group.Count() > 1);

        if (duplicateEvent is not null)
            throw new DuplicateEventStoreEventsFoundException(duplicateEvent.First().Item1);

        return eventTypeInformation;
    }

    public static void AddEventTypes(params Assembly[] assemblies)
    {
        var eventTypeInformation = GetEventTypeInformation(assemblies);

        foreach (var (key, type) in eventTypeInformation)
        {
            if (Types.ContainsKey(key))
                throw new DuplicateEventStoreEventsFoundException(key);

            Types[key] = type;
        }
    }
}