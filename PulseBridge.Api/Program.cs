using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PulseBridge.Api.SignalR;
using PulseBridge.Contracts;

var builder = WebApplication.CreateBuilder(args);

// SignalR
builder.Services.AddSignalR();


var app = builder.Build();

app.UseRouting();

// Minimal sanity routes
app.MapGet("/", () => Results.Ok("API up"));
app.MapGet("/health", () => Results.Ok("healthy"));

app.MapPost("/api/external/send", async ([FromBody] JobPayload payload, IHubContext<SchedulerHub, ISchedulerClient> hub) => {
    await hub.Clients.All.ReceiveMessage("signalr-user", payload.Message);
    return Results.Ok("sent");
});

// Map the hub and explicitly require CORS
app.MapHub<SchedulerHub>("/hubs/schedulerHub");



app.Run();
