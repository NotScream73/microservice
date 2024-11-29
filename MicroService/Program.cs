using Domain.Data;
using MicroService;
using MicroService.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Подключение к PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

builder.Host.UseSerilog((host, config) =>
{
    config.ReadFrom.Configuration(host.Configuration);
    config.WriteTo.File(
            "logs/main-log-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        );
});

builder.Services.AddScoped<ApplicationDbContextInitialiser>();

builder.Services.AddHttpClient();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
await app.InitialiseDatabaseAsync();
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseMiddleware<TraceLoggingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
