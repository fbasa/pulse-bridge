using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PulseBridge.Api.SignalR;
using PulseBridge.Contracts;

var builder = WebApplication.CreateBuilder(args);

// SignalR
builder.Services.AddSignalR();

// CORS (adjust origins to match your frontend dev URLs)
const string CorsPolicy = "Client";
builder.Services.AddCors(o =>
{
    o.AddPolicy(CorsPolicy, p => p
        .WithOrigins(
            "http://localhost:4200",  // Angular dev server
            "http://localhost:8081",  // SPA via nginx container
            "http://ui.localtest.me" // optional: if serving SPA on a hostname later
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());   //signalr requires this
});


var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseRouting();
app.UseCors(CorsPolicy);


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
