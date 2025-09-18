using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PulseBridge.Api.Caching;
using PulseBridge.Api.SignalR;
using PulseBridge.Contracts;
using PulseBridge.Infrastructure;
using StackExchange.Redis;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PulseBridge.Api.Auth;

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

var authAuthority = builder.Configuration["Auth:Issuer"];
var audience = "accounting-api";

builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddSingleton<IJobQueueRepository, JobQueueRepository>();

// AuthN
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authAuthority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = audience,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        options.RequireHttpsMetadata = true;
        options.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        // Important: allow token via query for WebSockets (SignalR client uses this)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var path = ctx.HttpContext.Request.Path;
                // Adjust if your hub path differs
                if (path.StartsWithSegments("/hubs/schedulerHub") &&
                    ctx.Request.Query.TryGetValue("access_token", out var token))
                { ctx.Token = token; }
                return Task.CompletedTask;
            }
        };
        // Optional: map "scope" => ClaimTypes.Role etc. Keep "scope" as-is for policy checks
    });

builder.Services.AddScopePolicies();
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("spa", p => p
        .WithOrigins("https://ui.localtest.me")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// SignalR
builder.Services.AddSignalR();

// MediatR 
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<Program>())
    // MediatR pipelines
    .AddTransient(typeof(IPipelineBehavior<,>), typeof(QueryCacheBehavior<,>));

// set ConnectionStrings:Redis
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

// Fallback to in-memory cache
builder.Services.AddMemoryCache();

// Output caching
builder.Services.AddOutputCache(options =>
{
    // Terms listing tag so we can evict on writes
    options.AddPolicy("joblist", b => b
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("jobs"));
});

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("api"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());
        
var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseRouting();
// output caching
app.UseOutputCache();

app.Use(async (ctx, next) =>
{
    // OWASP-ish headers
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

app.UseCors("spa");
app.UseAuthentication();
app.UseAuthorization();


//app.MapPost("/api/external/send", async ([FromBody] JobPayload payload, IHubContext<SchedulerHub, ISchedulerClient> hub) =>
//{
//   await hub.Clients.All.ReceiveMessage("signalr-user", payload.Message);
//   return Results.Ok("sent");
//});

app.MapControllers();

// Minimal sanity routes
app.MapGet("/health/ready", () => Results.Ok("API up"));

// Map the hub and explicitly require CORS
app.MapHub<SchedulerHub>("/hubs/schedulerHub").RequireAuthorization("accounting.read");//TODO

logger.Information("SignalR-API up and running!");

app.Run();
