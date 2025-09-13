using MediatR;
using Microsoft.AspNetCore.Mvc;
using PulseBridge.Api.Caching;
using PulseBridge.Contracts;
using PulseBridge.Infrastructure;

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


public sealed record SignalRJobsQuery() : IRequest<IEnumerable<SignalRJob>>, ICacheableQuery
{
    public string CacheKey => "jobs:all";
    public TimeSpan? Ttl => TimeSpan.FromSeconds(30);
}

public sealed class SignalRJobsQueryHandler(IJobQueueRepository repo) : IRequestHandler<SignalRJobsQuery, IEnumerable<SignalRJob>>
{
    public async Task<IEnumerable<SignalRJob>> Handle(SignalRJobsQuery request, CancellationToken ct)
    {
        return await repo.GetSignalRJobsAsync(ct);
    }
}

public sealed record SignalRJobCommand() : IRequest<int> { }

public sealed class SinalRJobCommandHandler(IJobQueueRepository repo) : IRequestHandler<SignalRJobCommand, int>
{
    public async Task<int> Handle(SignalRJobCommand request, CancellationToken ct)
    {
        return await repo.InjsertSignalRJobAsync(ct);
    }
}
