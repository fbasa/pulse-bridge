using Microsoft.Extensions.Options;
using PulseBridge.Infrastructure;

namespace PulseBridge.Worker;

public sealed class HttpGetJobHandler(
    IHttpClientFactory http,
    IJobQueueRepository repo,
    IOptions<AppOptions> opts) : IJobHandler
{
    public string JobType => "http-get";
    public async Task HandleAsync(long jobId, string payload, CancellationToken ct)
    {
        // here you would switch on msg.JobType and route to proper handler
        var client = http.CreateClient("external-api");

        try
        {
            using var resp = await client.PostAsJsonAsync(opts.Value.SendAndReceiveUrl, payload, ct);
            resp.EnsureSuccessStatusCode();

            // mark done (the worker, not the scheduler, owns completion)
            await repo.MarkJobCompletedAsync(jobId, ct);
        }
        catch (Exception ex)
        {
            // Return to pending with backoff; keep error for diagnostics
            await repo.RequeueWithBackoffAsync(jobId, ex.Message, 1, ct);
        }
    }
}
