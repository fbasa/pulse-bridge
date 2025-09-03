using MassTransit;
using PulseBridge.Infrastructure;
using PulseBridge.Scheduler;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddSingleton<IJobQueueRepository, JobQueueRepository>();

builder.Services.AddQuartz(q =>
{
    q.UsePersistentStore(s =>
    {
        s.UseSqlServer(x => x.ConnectionString = builder.Configuration.GetConnectionString("QuartzNet")!);
        s.UseProperties = true;
        s.UseClustering();
        s.UseNewtonsoftJsonSerializer();
    });

    var opts = builder.Configuration.GetSection(AppOptions.SectionName).Get<AppOptions>()!;
    var key = new JobKey("dequeue-and-publish", opts.WorkerGroup);

    q.AddJob<DequeueAndPublishHttpJob>(o => o.WithIdentity(key).StoreDurably());
    q.AddTrigger(t => t.ForJob(key)
        .WithIdentity("dequeue-and-publish-trigger", opts.WorkerGroup)
        .StartNow()
        .WithSimpleSchedule(s => s.WithInterval(TimeSpan.FromSeconds(opts.IntervalInSeconds)).RepeatForever()));
});
builder.Services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);
builder.Services.AddTransient<DequeueAndPublishHttpJob>();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((_, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:User"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Pass"] ?? "guest");
        });
    });
});

var app = builder.Build();
app.MapGet("/", () => "Scheduler up");
app.Run();
