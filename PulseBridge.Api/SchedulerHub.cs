using Microsoft.AspNetCore.SignalR;

namespace PulseBridge.Api.SignalR;

public record JobPayload(string Message);

public interface ISchedulerClient
{
    Task ReceiveMessage(string user, string message);
}

public class SchedulerHub : Hub<ISchedulerClient> { }