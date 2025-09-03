namespace PulseBridge.Contracts;

public sealed record JobRecord(long JobId, string JobType, string Payload, int Attempts);
