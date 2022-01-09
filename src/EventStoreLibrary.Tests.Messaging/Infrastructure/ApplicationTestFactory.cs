using EventStore.Library.Core;
using EventStore.Library.Messaging.Registration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EventStoreLibrary.Tests.Messaging.Infrastructure;

internal class ApplicationTestFactory : WebApplicationFactory<TestStartUp>, IApplicationTestFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddEventStore(new EventStoreClientOptions
                    {
                        ConnectionStringGrcp = "esdb://127.0.0.1:2113?tls=false&tlsVerifyCert=false",
                        ConnectionStringHttp = "http://127.0.0.1:2113",
                        Password = "changeit",
                        Username = "admin"
                    },
                    typeof(UnitTest1))
                .AddEventStoreMessaging(typeof(UnitTest1), options => options.UseInMemoryMessageStatusStorage())
                .AddEventStoreMessageBus();
        });
    }
}