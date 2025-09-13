﻿namespace PulseBridge.Infrastructure;

public sealed class AppOptions
{
    public const string SectionName = "App";

    public int ClaimBatchSize { get; init; } = 25;           // how many rows to claim per tick
    public int MaxPublishConcurrency { get; init; } = 15;    // parallel publishes
    public string WorkerGroup { get; init; } = "app";        // Quartz group for keys
    public bool UseRabbitMQ { get; init; } = false;          // enable RabbitMQ bus
    public int IntervalInSeconds { get; init; } = 30;
    public string? SendAndReceiveUrl { get; set; } // relative URL for sending messages
}