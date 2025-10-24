using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Extensions;
using UserService.Filters;
using UserService.Middlewares;
using UserService.Model.Entity;
using Serilog;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "unknown-service";

if (environment == "Docker")
{
    builder.Configuration.AddJsonFile(
        "appsettings.Docker.json",
        optional: true,
        reloadOnChange: true
    );

    builder.AddMonitoring();
}
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(option =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException(
            "Connection string 'DefaultConnection' is null or empty."
        );
    }
    option.UseMySQL(connectionString);
});

builder.Services.AddCustomCors();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.SuppressModelStateInvalidFilter = true
);

builder
    .Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilterAttribute>();
});

builder.Services.AddKafkaServices(builder.Configuration);
builder.Services.AddCustomServices(builder.Configuration);
builder.AddAuthenticationAndAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var meter = new Meter("custom_metrics_"+serviceName, "1.0");
var responseSizeCounter = meter.CreateCounter<long>("http_response_size_bytes");
app.Use(async (context, next) =>
{
    var originalBodyStream = context.Response.Body;
    using var memoryStream = new MemoryStream();
    context.Response.Body = memoryStream;

    await next();

    var size = memoryStream.Length;
    responseSizeCounter.Add(size, new KeyValuePair<string, object?>("service_name", serviceName));

    memoryStream.Seek(0, SeekOrigin.Begin);
    await memoryStream.CopyToAsync(originalBodyStream);
    context.Response.Body = originalBodyStream;
});
var uniqueVisitorCounter = meter.CreateCounter<long>("unique_visitors");

app.Use(async (context, next) =>
{
    await next();

    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var browser = context.Request.Headers["User-Agent"].ToString() ?? "unknown";
    var timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm"); // group by minute

    // Use tags as labels
    uniqueVisitorCounter.Add(1, new KeyValuePair<string, object?>("ip", ip),
                               new KeyValuePair<string, object?>("browser", browser),
                               new KeyValuePair<string, object?>("timestamp", timestamp));
});

if (!app.Environment.IsEnvironment("Testing") && environment == "Docker")
{
    app.UseSerilogRequestLogging(options =>
    {
        options.IncludeQueryInRequestPath = true;
    });

    app.UseRequestResponseLogging();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseExceptionHandler(builder =>
{
    builder.Run(async context =>
    {
        var exceptionHandler = context.RequestServices.GetRequiredService<IExceptionHandler>();
        var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

        if (exception != null)
        {
            await exceptionHandler.TryHandleAsync(context, exception, context.RequestAborted);
        }
    });
});

app.UseCors(CorsExtensions.GetCorsPolicyName());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.ApplyPendingMigrations();
app.Run();
