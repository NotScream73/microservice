using Amazon.Runtime;
using Amazon.S3;
using Serilog;
using StorageService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(
    new BasicAWSCredentials("minioadmin", "minioadmin"),
    new AmazonS3Config
    {
        ServiceURL = "http://minio:9000",  // URL MinIO
        ForcePathStyle = true              // Используем путь вместо DNS
    }
));

builder.Host.UseSerilog((host, config) =>
{
    config.ReadFrom.Configuration(host.Configuration);
    config.WriteTo.File(
            "logs/storage-log-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        );
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseMiddleware<TraceLoggingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
