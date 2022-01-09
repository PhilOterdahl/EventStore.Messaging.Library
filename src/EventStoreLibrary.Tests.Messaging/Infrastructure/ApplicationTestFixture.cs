using System;
using System.Threading.Tasks;

namespace EventStoreLibrary.Tests.Messaging.Infrastructure;

internal class ApplicationTestFixture<TApplicationTestFactory> : IApplicationFixture
    where TApplicationTestFactory : class, IApplicationTestFactory, new()
{
    protected TApplicationTestFactory Factory { get; private set; }

    public IServiceProvider Services => Factory.Services;

    public virtual Task InitializeEnvironment()
    {
        return Task.CompletedTask;
    }

    public virtual Task InitializeTest()
    {
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await InitializeEnvironment();
        Factory = new TApplicationTestFactory();
        await InitializeTest();
    }
}