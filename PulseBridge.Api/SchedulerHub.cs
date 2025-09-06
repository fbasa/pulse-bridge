using Microsoft.AspNetCore.SignalR;

namespace PulseBridge.Api.SignalR;

public record JsonPayload(string Message);

public interface ISchedulerClient
{
    Task ReceiveMessage(string user, string message);
}

public class SchedulerHub : Hub<ISchedulerClient> { }