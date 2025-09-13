using System.Net;
using Microsoft.Extensions.Options;
using PulseBridge.Contracts;
using PulseBridge.Infrastructure;

namespace PulseBridge.Worker;
public sealed class HttpGetJobHandler(
    IHttpClientFactory http,
    IJobQueueRepository repo,
    IOptions<AppOptions> opts) : IJobHandler
{
    public string JobType => "SignalR";
    public async Task HandleAsync(long jobId, string payload, CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(opts.Value.SendAndReceiveUrl))
        {
            throw new InvalidOperationException("API url is not configured");
        };
        
        // here you would switch on msg.JobType and route to proper handler
        var client = http.CreateClient("external-api");

        using var resp = await client.PostAsJsonAsync(opts.Value.SendAndReceiveUrl, new JobPayload(payload), ct);

        if (resp.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity or HttpStatusCode.NotFound or HttpStatusCode.Conflict)
        {
            // Permanent (do NOT retry): dead-letter immediately
            var body = await resp.Content.ReadAsStringAsync();
            throw new UnrecoverableMessageException($"Permanent failure from API {(int)resp.StatusCode}: {body}");
        }
        
        resp.EnsureSuccessStatusCode();

        // mark done (the worker, not the scheduler, owns completion)
        await repo.MarkJobCompletedAsync(jobId, ct);

    }
}
