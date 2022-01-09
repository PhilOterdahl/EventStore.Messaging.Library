using EventStore.Library.Core;
using EventStore.Library.Examples.Messaging.Processes;
using EventStore.Library.Messaging.Registration;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services
    .AddEventStore(new EventStoreClientOptions
        {
            ConnectionStringGrcp = "esdb://127.0.0.1:2113?tls=false&tlsVerifyCert=false",
            ConnectionStringHttp = "http://127.0.0.1:2113",
            Password = "changeit",
            Username = "admin"
        },
        typeof(EnrollUserCommand))
    .AddEventStoreMessaging(typeof(EnrollUserCommand), options => options.UseInMemoryMessageStatusStorage())
    .AddEventStoreMessageBus()
    .AddSwaggerDocument(settings => settings.Title = "Event Store Library Messaging Example")
    .AddControllers();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseOpenApi();
    app.UseSwaggerUi3();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
