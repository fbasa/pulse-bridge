namespace PulseBridge.Worker;

public interface IJobHandler
{
    string JobType { get; }                 // e.g., "http-get", "send-email"
    Task HandleAsync(long jobId, string payload, CancellationToken ct);
}