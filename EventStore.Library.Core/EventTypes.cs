using System.Collections.ObjectModel;
using System.Reflection;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Core;

internal static class EventTypes
{
    public static IReadOnlyDictionary<string, Type> Types { get; private set; } = new ReadOnlyDictionary<string, Type>(new Dictionary<string, Type>());

    public static void SetEventTypes(Assembly[] assemblies)
    {
        Types = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract)
            .Where(type => !type.IsGenericType)
            .Where(type => type.IsAssignableFrom(typeof(IEventStoreEvent)))
            .ToDictionary(type => type.Name);
    }
}