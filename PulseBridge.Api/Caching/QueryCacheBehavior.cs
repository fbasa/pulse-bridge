using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace PulseBridge.Api.Caching;

public sealed class QueryCacheBehavior<TRequest, TResponse>(IDistributedCache dist, IMemoryCache mem)
  : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is not ICacheableQuery cq) return await next();

        // Prefer distributed cache if configured; fall back to in-memory
        if (dist is not null)
        {
            var cached = await dist.GetStringAsync(cq.CacheKey, ct);
            if (cached is not null) return JsonSerializer.Deserialize<TResponse>(cached)!;

            var resp = await next();
            var ttl = cq.Ttl ?? TimeSpan.FromSeconds(30);
            await dist.SetStringAsync(cq.CacheKey, JsonSerializer.Serialize(resp),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, ct);
            return resp;
        }
        else
        {
            if (mem.TryGetValue(cq.CacheKey, out TResponse hit)) return hit!;
            var resp = await next();
            mem.Set(cq.CacheKey, resp, cq.Ttl ?? TimeSpan.FromSeconds(30));
            return resp;
        }
    }
}
