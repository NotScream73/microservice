using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOcelot(builder.Configuration);

// Add Ocelot json file configuration
builder.Configuration.AddJsonFile("ocelot.json");

var app = builder.Build();

await app.UseOcelot();

await app.RunAsync();