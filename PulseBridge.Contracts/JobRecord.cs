namespace PulseBridge.Contracts;

public record JobPayload(string Message);
public sealed record JobRecord(long JobId, string JobType, string Payload, int Attempts);
