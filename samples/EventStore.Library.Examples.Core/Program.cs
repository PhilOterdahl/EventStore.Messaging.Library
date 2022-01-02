using EventStore.Library.Core;
using EventStore.Library.Examples.Core.Aggregates;

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
        typeof(ShoppingCart))
    .AddSwaggerDocument(settings => settings.Title = "Event Store Library Core Example")
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
