using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PulseBridge.Api.SignalR;

var builder = WebApplication.CreateBuilder(args);

// SignalR
builder.Services.AddSignalR();

// CORS (adjust origins to match your frontend dev URLs)
const string CorsPolicy = "Client";
builder.Services.AddCors(o =>
{
    o.AddPolicy(CorsPolicy, p => p
        .WithOrigins(
            "http://localhost:4200"   // Angular
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());   //signalr requires this
});


var app = builder.Build();

app.UseCors(CorsPolicy);


// Minimal sanity route
app.MapGet("/", () => Results.Ok("API up"));

app.MapPost("/api/external/send", async ([FromBody] JobPayload payload, IHubContext<SchedulerHub, ISchedulerClient> hub) => {
    await hub.Clients.All.ReceiveMessage("signalr-user", payload.Message);
    return Results.Ok("sent");
});

// Map the hub
app.MapHub<SchedulerHub>("/hubs/schedulerHub");



app.Run();
