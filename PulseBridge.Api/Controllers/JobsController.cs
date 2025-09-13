using MediatR;
using Microsoft.AspNetCore.Mvc;
using PulseBridge.Api.Handlers;

namespace PulseBridge.Api.Controllers;
public sealed class JobsController : BaseApiController
{
    public JobsController(ISender sender) : base(sender) { }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct = default)
    {
        return Ok(await Sender.Send(new SignalRJobsQuery(), ct));
    }

    [HttpGet("insert")]
    public async Task<IActionResult> InsertJob(CancellationToken ct = default)
    {
        return Ok(await Sender.Send(new SignalRJobCommand(), ct));
    }
}