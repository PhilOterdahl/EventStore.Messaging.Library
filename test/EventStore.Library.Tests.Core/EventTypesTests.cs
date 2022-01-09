using System.Linq;
using EventStore.Library.Core;
using EventStore.Library.Tests.Core.TestAggregates;
using EventStore.Library.Tests.Core.TestAggregates.Events;
using FluentAssertions;
using Xunit;

namespace EventStore.Library.Tests.Core;

public class EventTypesTests
{
    [Fact]
    public void Event_types_class_registers_all_events_inheriting_from_ievent_store_event_in_specified_assemblies()
    {
        var assemblies = new[] { typeof(User).Assembly };
        EventTypes.SetEventTypes(assemblies);

        var userEvents = EventTypes.GetTypes(typeof(IUserEvent));
        userEvents.ToArray().Should().BeEquivalentTo(new[]
        {
            typeof(UserAccountLockedEvent),
            typeof(TestAggregates.Archived.UserAccountLockedEvent),
            typeof(UserContactInformationEditedEvent),
            typeof(UserEnrolledEvent),
        });
    }

    [Fact]
    public void Event_types_class_registers_events_with_same_name_if_they_have_different_versions()
    {
        var assemblies = new[] { typeof(User).Assembly };
        EventTypes.SetEventTypes(assemblies);

        var userEvents = EventTypes.GetTypes(typeof(IUserEvent));
        userEvents.ToArray().Should().BeEquivalentTo(new[]
        {
            typeof(UserAccountLockedEvent),
            typeof(TestAggregates.Archived.UserAccountLockedEvent),
            typeof(UserContactInformationEditedEvent),
            typeof(UserEnrolledEvent),
        });
    }
}