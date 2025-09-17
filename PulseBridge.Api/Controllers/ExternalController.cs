using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PulseBridge.Api.SignalR;
using PulseBridge.Contracts;

namespace PulseBridge.Api.Controllers;

[Route("api/[controller]")]
public class ExternalController(IHubContext<SchedulerHub, ISchedulerClient> hub) : Controller
{
    [HttpPost("send")]
    public async Task<IActionResult> SendPayloadAsync([FromBody] JobPayload payload)
    {
        await hub.Clients.All.ReceiveMessage("signalr-user", payload.Message);
        return Ok("sent");
    }
}
