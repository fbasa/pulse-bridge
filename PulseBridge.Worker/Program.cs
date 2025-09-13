using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PulseBridge.Infrastructure;
using PulseBridge.Worker;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProcessJobConsumer>(c => c.ConcurrentMessageLimit = 32);

    x.UsingRabbitMq((context, cfg) =>
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
app.MapGet("/", () => "Worker up");
app.Run();
