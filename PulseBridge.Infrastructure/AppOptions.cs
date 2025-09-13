namespace PulseBridge.Infrastructure;

public sealed class AppOptions
{
    public const string SectionName = "App";

    public int ClaimBatchSize { get; set; } = 25;           // how many rows to claim per tick
    public int MaxPublishConcurrency { get; set; } = 15;    // parallel publishes
    public string WorkerGroup { get; set; } = "app";        // Quartz group for keys
    public int IntervalInSeconds { get; set; } = 35;
    public string? SendAndReceiveUrl { get; set; } // absolute URL for sending messages
}