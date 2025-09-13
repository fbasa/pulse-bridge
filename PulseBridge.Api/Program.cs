using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using PulseBridge.Api.Caching;
using PulseBridge.Api.SignalR;
using PulseBridge.Contracts;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// MVC (controllers)
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);

// SignalR
builder.Services.AddSignalR();

// MediatR 
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<Program>())
    // MediatR pipelines
    .AddTransient(typeof(IPipelineBehavior<,>), typeof(QueryCacheBehavior<,>));

// Optional distributed cache (Redis): set ConnectionStrings:Redis to enable
var redisCs = config.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisCs))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var cfg = ConfigurationOptions.Parse(redisCs, true);
        cfg.AbortOnConnectFail = false;
        cfg.ConnectRetry = 3;
        cfg.SyncTimeout = 5000;
        return ConnectionMultiplexer.Connect(cfg);
    }).AddSingleton<IDistributedCache>(sp =>
    {
        return new RedisCache(new RedisCacheOptions
        {
            // Reuse the existing multiplexer instead of creating a new one
            ConnectionMultiplexerFactory = () => Task.FromResult(sp.GetRequiredService<IConnectionMultiplexer>()),
            InstanceName = "pulse-bridge:" // key prefix
        });
    });
}

// Output caching
builder.Services.AddOutputCache(options =>
{
    // Terms listing � tag so we can evict on writes
    options.AddPolicy("joblist", b => b
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("jobs"));
});

var app = builder.Build();

app.UseRouting();
// output caching
app.UseOutputCache();


// Minimal sanity routes
app.MapGet("/", () => Results.Ok("API up"));

app.MapPost("/api/external/send", async ([FromBody] JobPayload payload, IHubContext<SchedulerHub, ISchedulerClient> hub) =>
{
    await hub.Clients.All.ReceiveMessage("signalr-user", payload.Message);
    return Results.Ok("sent");
});

app.MapControllers();

// Map the hub and explicitly require CORS
app.MapHub<SchedulerHub>("/hubs/schedulerHub");

app.Run();
