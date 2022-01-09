using System;

namespace EventStoreLibrary.Tests.Messaging.Infrastructure;

internal interface IApplicationTestFactory
{
    IServiceProvider Services { get; }
}