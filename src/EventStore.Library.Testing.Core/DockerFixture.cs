using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using Xunit;

namespace EventStore.Library.Testing.Core;

public abstract class DockerFixture : IAsyncLifetime
{
    public IService Containers { get; set; }

    public bool Initialized { get; private set; }

    protected abstract IBuilder ConfigureContainers();

    public Task InitializeAsync()
    {
        try
        {
            Containers = ConfigureContainers().Build();
            Containers.Start();
            Initialized = true;
        }
        catch (Exception)
        {
            Containers.Dispose();
            throw;
        }

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Containers?.Stop();
        Containers?.Remove(true);
        Containers?.Dispose();
        return Task.CompletedTask;
    }
}