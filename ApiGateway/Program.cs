using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOcelot(builder.Configuration);

// Add Ocelot json file configuration
builder.Configuration.AddJsonFile("ocelot.json");

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Trace-Id"))
    {
        context.Request.Headers["X-Trace-Id"] = Guid.NewGuid().ToString();
    }
    await next.Invoke();
});

await app.UseOcelot();

await app.RunAsync();