using MassTransit;
using PulseBridge.Contracts;
using PulseBridge.Infrastructure;

namespace PulseBridge.Worker;

public sealed class ProcessJobConsumer(
    IJobHandlerRegistry registry,
    IJobQueueRepository repo,
    ILogger<ProcessJobConsumer> log) : IConsumer<ProcessJob>
{
    public async Task Consume(ConsumeContext<ProcessJob> ctx)
    {
        var msg = ctx.Message;
        var ct = ctx.CancellationToken;

        var handler = registry.Resolve(msg.JobType);
        if (handler is null)
        {
            await repo.RequeueWithBackoffAsync(msg.JobId, $"No handler for '{msg.JobType}'", msg.Attempts, ct);
            // ack: do not throw; DB backoff governs retry
            return;
        }

        try
        {
            await handler.HandleAsync(msg.JobId, msg.Payload, ct);
            await repo.MarkJobCompletedAsync(msg.JobId, ct);
        }
        catch (Exception ex)
        {
            // requeue via DB backoff; ACK message to avoid duplicate redelivery storms
            await repo.RequeueWithBackoffAsync(msg.JobId, ex.Message, msg.Attempts, ct);
            // optional: log/notify; do NOT throw
            log.LogWarning(ex, "Job {JobId} failed; requeued", msg.JobId);
        }
    }
}