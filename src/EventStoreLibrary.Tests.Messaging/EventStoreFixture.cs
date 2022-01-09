using System;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Extensions;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Extensions;
using EventStore.Library.Testing.Core;
using Xunit;

namespace EventStoreLibrary.Tests.Messaging;

public class EventStoreFixture : DockerFixture
{
    protected override IBuilder ConfigureContainers() => new Builder()
        .UseContainer()
        .UseImage("eventstore/eventstore:21.10.0-buster-slim")
        .WithEnvironment(
            "EVENTSTORE_CLUSTER_SIZE=1",
            "EVENTSTORE_RUN_PROJECTIONS=All",
            "EVENTSTORE_START_STANDARD_PROJECTIONS=true",
            "EVENTSTORE_EXT_TCP_PORT=1113",
            "EVENTSTORE_HTTP_PORT=2113",
            "EVENTSTORE_INSECURE=true",
            "EVENTSTORE_ENABLE_EXTERNAL_TCP=true",
            "EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP=true",
            "EVENTSTORE_MEM_DB=true")
        .ExposePort(2113)
        .ExposePort(1113)
        .WaitForPort("2113/tcp", TimeSpan.FromSeconds(30));

    public void RestEventStore()
    {
        var service = (IContainerService)Containers;
        service.Stop();
        service.WaitForStopped();
        service.Start();
        service.WaitForRunning();
        service.WaitForPort("2113/tcp");
    }
}

[CollectionDefinition("EventStore")]
public class EventStoreCollection : ICollectionFixture<EventStoreFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
