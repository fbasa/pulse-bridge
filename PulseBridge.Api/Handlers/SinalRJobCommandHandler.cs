using MediatR;
using PulseBridge.Api.Caching;
using PulseBridge.Infrastructure;

namespace PulseBridge.Api.Handlers;

public sealed record SignalRJobCommand() : IRequest<int> { }

public sealed class SinalRJobCommandHandler(IMediator mediator, IJobQueueRepository repo) : IRequestHandler<SignalRJobCommand, int>
{
    public async Task<int> Handle(SignalRJobCommand request, CancellationToken ct)
    {
        var newId = await repo.InjsertSignalRJobAsync(ct);
        await mediator.Publish(new JobsChanged(), ct);       // invalidate cached
        return newId;
    }
}