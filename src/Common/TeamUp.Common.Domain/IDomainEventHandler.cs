using MediatR;

namespace TeamUp.Common.Domain;

public interface IDomainEventHandler<TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{
	public new Task Handle(TDomainEvent domainEvent, CancellationToken ct);
}
