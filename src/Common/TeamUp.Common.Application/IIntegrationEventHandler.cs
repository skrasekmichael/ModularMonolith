using RailwayResult;

using TeamUp.Common.Contracts;

namespace TeamUp.Common.Application;

public interface IIntegrationEventHandlerMarker;

public interface IIntegrationEventHandler<TIntegrationEvent> : IIntegrationEventHandlerMarker where TIntegrationEvent : class, IIntegrationEvent
{
	public Task<Result> Handle(TIntegrationEvent integrationEvent, CancellationToken ct);
}
