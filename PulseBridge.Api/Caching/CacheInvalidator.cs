using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace PulseBridge.Api.Caching;

public record JobsChanged() : INotification;

public sealed class CacheInvalidator(IDistributedCache dist, IMemoryCache mem, ILogger<CacheInvalidator> logger) : INotificationHandler<JobsChanged>
{
    public async Task Handle(JobsChanged j, CancellationToken ct)
    {
        // list keys are many; rely on short TTL (30s) to avoid complex tag invalidation
        if (dist is not null)
        {
            logger.LogInformation($"Removing cached -> {CacheKeys.JobsAll}");
            await dist.RemoveAsync(CacheKeys.JobsAll, ct);
        }
        else
        {
            mem.Remove(CacheKeys.JobsAll);
        }
    }
}
