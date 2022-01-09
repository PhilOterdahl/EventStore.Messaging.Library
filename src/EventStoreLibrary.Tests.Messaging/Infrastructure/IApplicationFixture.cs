using System;

namespace EventStoreLibrary.Tests.Messaging.Infrastructure;

internal interface IApplicationFixture
{
    public IServiceProvider Services { get; }
}