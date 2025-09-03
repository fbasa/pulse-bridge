namespace PulseBridge.Worker;

public sealed class JobHandlerRegistry(IEnumerable<IJobHandler> handlers) : IJobHandlerRegistry
{
    private readonly Dictionary<string, IJobHandler> _map =
        handlers.ToDictionary(h => h.JobType, StringComparer.OrdinalIgnoreCase);

    public IJobHandler? Resolve(string jobType) => _map.TryGetValue(jobType, out var h) ? h : null;
}