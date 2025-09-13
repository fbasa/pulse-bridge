using Dapper;
using PulseBridge.Contracts;

namespace PulseBridge.Infrastructure;

public sealed class JobQueueRepository(IDbConnectionFactory factory) : IJobQueueRepository
{
    public async Task<IReadOnlyList<JobRecord>> ClaimAsync(int batch, string workerId, CancellationToken ct)
    {
        var result = await factory.QueryWithRetryAsync<JobRecord>(
                commandText: SqlTemplates.ClaimJobs,
                parameters: new { batch, worker = workerId },
                cancellationToken: ct
            );
        return result.AsList();
    }

    public async Task RequeueWithBackoffAsync(long jobId, string error, int attempts, CancellationToken ct)
    {
        await factory.ExecuteWithRetryAsync(
             commandText: SqlTemplates.RequeWithBackoff,
             parameters: new { jobId, error, attempts },
             cancellationToken: ct
         );
    }

    public async Task MarkDispatchedAsync(long jobId, CancellationToken ct)
    {
        await factory.ExecuteWithRetryAsync(
             commandText: SqlTemplates.MarkDispatched,
             parameters: new { jobId },
             cancellationToken: ct
         );
    }

    public async Task MarkJobCompletedAsync(long jobId, CancellationToken ct)
    {
        await factory.ExecuteWithRetryAsync(
             commandText: SqlTemplates.MarkJobCompleted,
             parameters: new { jobId },
             cancellationToken: ct
         );
    }

    public async Task<IReadOnlyList<SignalRJob>> GetSignalRJobsAsync(CancellationToken ct)
    {
        var result = await factory.QueryWithRetryAsync<SignalRJob>(
                commandText: SqlTemplates.GetSignalRJobs,
                cancellationToken: ct
            );
        return result.AsList();
    }

    public async Task<int> InjsertSignalRJobAsync(CancellationToken ct)
    {
        return await factory.ExecuteWithRetryAsync(
             commandText: SqlTemplates.InsertSignalRJob,
             cancellationToken: ct
         );
    }
}
