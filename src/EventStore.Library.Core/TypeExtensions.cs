using System.Reflection;
using EventStore.Library.Core.Domain.Aggregate;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Core;

public static class TypeExtensions
{
    public static string GetEventType(this Type type)
    {
        if (!type.IsAssignableTo(typeof(IEventStoreEvent)))
            throw new InvalidOperationException(
                "Can not get event store type, type is not inheriting from IEventStoreEvent");

        var eventAttribute = type.GetCustomAttribute<EventStoreEventAttribute>();
        var typeName = eventAttribute?.Name ?? type.Name;
        var version = eventAttribute?.Version == 0 ? null : eventAttribute?.Version;
        return version is not null
            ? $"{typeName}-v{version}"
            : typeName;
    }
}