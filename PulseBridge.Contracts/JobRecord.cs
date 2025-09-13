namespace PulseBridge.Contracts;

public record JobPayload(string Message);
public sealed record JobRecord(long JobId, string JobType, string Payload, int Attempts);
public sealed class SignalRJob {
    public long JobId { get; set; }
    public string JobType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public int Status { get; set; }
};
