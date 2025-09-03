using MassTransit;
using PulseBridge.Infrastructure;
using PulseBridge.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddSingleton<IJobQueueRepository, JobQueueRepository>();

builder.Services.AddHttpClient("external-api")
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(10));

builder.Services.AddSingleton<IJobHandler, HttpGetJobHandler>();
builder.Services.AddSingleton<IJobHandler, SendEmailJobHandler>();
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