using System.Text.Json;
using MassTransit;
using Polly;
using Polly.Retry;
using PulseBridge.Contracts;
using PulseBridge.Infrastructure;

namespace PulseBridge.Worker;

public sealed class ProcessJobConsumer(
    IJobHandlerRegistry registry,
    IJobQueueRepository repo,
    ILogger<ProcessJobConsumer> logger) : IConsumer<ProcessJob>
{

    private static AsyncRetryPolicy TransientRetry(ILogger logger) =>
        Policy.Handle<Exception>(ex =>
                ex is not UnrecoverableMessageException &&
                ex.GetBaseException() is not UnrecoverableMessageException)
              .WaitAndRetryAsync(
                  retryCount: 3,
                  sleepDurationProvider: i => TimeSpan.FromSeconds(Math.Pow(2, i)),
                  onRetry: (ex, delay, attempt, ctx) =>
                  {
                      logger.LogWarning(ex,
                          "API POST attempt {Attempt} failed. Retrying in {Delay}s…",
                          attempt, delay.TotalSeconds);
                  });

    public async Task Consume(ConsumeContext<ProcessJob> ctx)
    {
        var msg = ctx.Message;
        var ct = ctx.CancellationToken;

        // Validate payload (schema/shape). Treat invalid as *permanent*.
        if (!IsValidJson(msg.Payload))
        {
            logger.LogWarning("Invalid JSON payload.");
            return;
        }
            
        var handler = registry.Resolve(msg.JobType);
        if (handler is null)
        {
            await repo.RequeueWithBackoffAsync(msg.JobId, $"No handler for '{msg.JobType}'", msg.Attempts, ct);
            // ack: do not throw; DB backoff governs retry
            return;
        }

        try
        {
            await TransientRetry(logger).ExecuteAsync(async () =>
            {
                await handler.HandleAsync(msg.JobId, msg.Payload, ct);
                await repo.MarkJobCompletedAsync(msg.JobId, ct);
            });
        }
        catch (Exception ex)
        {
            // requeue via DB backoff; ACK message to avoid duplicate redelivery storms
            await repo.RequeueWithBackoffAsync(msg.JobId, ex.Message, msg.Attempts, ct);
            // optional: log/notify; do NOT throw
            logger.LogWarning(ex, "Job {JobId} failed; requeued", msg.JobId);
        }
    }

    static bool IsValidJson(string s)
    {
        if(s == null) return false;
        try { using var _ = JsonDocument.Parse(s); return true; }
        catch { return false; }
    }
}