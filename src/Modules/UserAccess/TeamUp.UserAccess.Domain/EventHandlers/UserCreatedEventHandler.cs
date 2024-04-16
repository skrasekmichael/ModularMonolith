using MassTransit;

using TeamUp.Common.Domain;
using TeamUp.UserAccess.Contracts.CreateUser;
using TeamUp.UserAccess.Domain.DomainEvents;

namespace TeamUp.UserAccess.Domain.EventHandlers;

internal sealed class UserCreatedEventHandler : IDomainEventHandler<UserCreatedDomainEvent>
{
	private readonly IPublishEndpoint _publishEndpoint;

	public UserCreatedEventHandler(IPublishEndpoint publishEndpoint)
	{
		_publishEndpoint = publishEndpoint;
	}

	public Task Handle(UserCreatedDomainEvent domainEvent, CancellationToken ct)
	{
		var message = new UserCreatedIntegrationEvent
		{
			UserId = domainEvent.User.Id,
			Email = domainEvent.User.Email,
			Name = domainEvent.User.Name
		};

		return _publishEndpoint.Publish(message, ct);
	}
}
