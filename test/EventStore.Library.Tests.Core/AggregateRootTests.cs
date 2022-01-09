using System;
using System.Threading.Tasks;
using EventStore.Library.Core;
using EventStore.Library.Core.Domain.Aggregate;
using EventStore.Library.Testing.Core;
using EventStore.Library.Tests.Core.TestAggregates;
using EventStore.Library.Tests.Core.TestAggregates.Events;
using FluentAssertions;
using Xunit;

namespace EventStore.Library.Tests.Core;

public class AggregateRootTests : TestBaseAggregateRoot<User, UserId, IUserEvent, UserState>
{
    public AggregateRootTests() : base(new User())
    {
    }

    [Fact]
    public void Creating_aggregate_root_that_is_already_created_throws_aggregate_root_already_created_Exception()
    {
       Given(new UserEnrolledEvent(UserId.Create(), "Pepe", "Sliva", "pepe.silva@hotmail.com", "Pepe Silva"));

       Throws<AggregateRootAlreadyCreatedException>(user =>  user.EnrollUser("Pepe", "Silva", "pepe.silva@hotmail.com", "Pepe Silva"));
    }

    [Fact]
    public async Task Aggregate_root_updates_last_updated_when_events_gets_added()
    {
        var userEnrolled = new UserEnrolledEvent(UserId.Create(), "Pepe", "Sliva", "pepe.silva@hotmail.com", "Pepe Silva");
        await Task.Delay(TimeSpan.FromSeconds(1));
        var contactInformationEdited = new UserContactInformationEditedEvent(userEnrolled.StreamId, "pepe.silva@gmail.com", "Pepe Silva");

        Given(userEnrolled, contactInformationEdited);

        Then(user => user.LastUpdated.Should().Be(contactInformationEdited.Date));
    }

    [Fact]
    public async Task Aggregate_root_updates_last_updated_by_when_events_gets_added()
    {
        var userEnrolled = new UserEnrolledEvent(UserId.Create(), "Pepe", "Sliva", "pepe.silva@hotmail.com", "Pepe Silva");
        await Task.Delay(TimeSpan.FromSeconds(1));
        var contactInformationEdited = new UserContactInformationEditedEvent(userEnrolled.StreamId, "pepe.silva@gmail.com", "John Doe");

        Given(userEnrolled, contactInformationEdited);

        Then(user => user.LastUpdatedBy.Should().Be(contactInformationEdited.By));
    }

    [Fact]
    public void Aggregate_root_sets_created_date_when_first_event_gets_added()
    {
        var userEnrolled = new UserEnrolledEvent(UserId.Create(), "Pepe", "Sliva", "pepe.silva@hotmail.com", "Pepe Silva");
        var contactInformationEdited = new UserContactInformationEditedEvent(userEnrolled.StreamId, "pepe.silva@gmail.com", "John Doe");
        Given(userEnrolled, contactInformationEdited);

        Then(user => ((UserState)user).Created.Should().Be(userEnrolled.Date));
    }

    [Fact]
    public void Aggregate_root_throws_domain_event_not_handled_exception_when_state_modification_action_is_not_found_and_ignore_state_attribute_is_not_applied_to_event()
    {
        var userEnrolled = new UserEnrolledEvent(UserId.Create(), "Pepe", "Sliva", "pepe.silva@hotmail.com", "Pepe Silva");
        Given(userEnrolled);

        Throws<DomainEventNotHandledException>(user => user.AccountLocked("Pepe Silva"));
    }
}