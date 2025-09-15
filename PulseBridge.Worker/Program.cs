using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PulseBridge.Infrastructure;
using PulseBridge.Worker;
using Serilog;

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddSingleton<IJobQueueRepository, JobQueueRepository>();
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));

builder.Services.AddHttpClient("external-api")
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(10));

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("worker"))
    .WithTracing(t =>
        t.AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

builder.Services.AddSingleton<IJobHandler, HttpGetJobHandler>();
builder.Services.AddSingleton<IJobHandlerRegistry, JobHandlerRegistry>();

builder.Services.AddMassTransit(busConfig =>
{
    busConfig.AddConsumer<ProcessJobConsumer>(c => c.ConcurrentMessageLimit = 32);

    busConfig.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:User"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Pass"] ?? "guest");
        });

        cfg.PrefetchCount = 64;                         // pull-ahead
        cfg.ConfigureEndpoints(context);                // "app-process-job" queue (kebab-case)
    });
});

var app = builder.Build();
app.UseSerilogRequestLogging();

app.MapGet("/", () => "Worker up");

logger.Information("Worker up and running!");

app.Run();
