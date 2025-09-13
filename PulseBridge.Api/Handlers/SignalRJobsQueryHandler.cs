using MediatR;
using PulseBridge.Api.Caching;
using PulseBridge.Contracts;
using PulseBridge.Infrastructure;

namespace PulseBridge.Api.Handlers;

public sealed record SignalRJobsQuery() : IRequest<IEnumerable<SignalRJob>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.JobsAll;
    public TimeSpan? Ttl => TimeSpan.FromSeconds(30);
}

public sealed class SignalRJobsQueryHandler(IJobQueueRepository repo) : IRequestHandler<SignalRJobsQuery, IEnumerable<SignalRJob>>
{
    public async Task<IEnumerable<SignalRJob>> Handle(SignalRJobsQuery request, CancellationToken ct)
    {
        return await repo.GetSignalRJobsAsync(ct);
    }
}
