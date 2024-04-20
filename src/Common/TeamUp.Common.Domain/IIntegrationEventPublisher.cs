using TeamUp.Common.Contracts;

namespace TeamUp.Common.Domain;

public interface IIntegrationEventPublisher<TModuleId> where TModuleId : IModuleId
{
	public void Publish<TEvent>(TEvent integrationEvent) where TEvent : class, IIntegrationEvent;
}
