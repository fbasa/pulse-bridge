using PulseBridge.Contracts;

namespace PulseBridge.Infrastructure;

public interface IJobQueueRepository
{
    Task<IReadOnlyList<JobRecord>> ClaimAsync(int batch, string workerId, CancellationToken ct);
    Task RequeueWithBackoffAsync(long jobId, string error, int attempts, CancellationToken ct);
    Task MarkDispatchedAsync(long jobId, CancellationToken ct);
    Task MarkJobCompletedAsync(long jobId, CancellationToken ct);
}