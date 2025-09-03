namespace PulseBridge.Worker;

public interface IJobHandlerRegistry
{
    IJobHandler? Resolve(string jobType);
}