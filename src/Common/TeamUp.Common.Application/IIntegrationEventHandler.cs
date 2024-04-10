using TeamUp.Domain.Abstractions;

namespace TeamUp.Common.Application;

public interface IIntegrationEventHandler<TIntegrationEvent> where TIntegrationEvent : IIntegrationEvent;
