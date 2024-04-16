using RailwayResult;

using TeamUp.Domain.Abstractions;

namespace TeamUp.Common.Application;

public interface IIntegrationEventHandler<TIntegrationEvent> where TIntegrationEvent : class, IIntegrationEvent
{
	public Task<Result> Handle(TIntegrationEvent integrationEvent, CancellationToken ct);
}
