using TeamUp.Common.Domain;
using TeamUp.Notifications.Contracts;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.CreateUser;
using TeamUp.UserAccess.Domain.DomainEvents;

namespace TeamUp.UserAccess.Domain.EventHandlers;

internal sealed class UserCreatedEventHandler : IDomainEventHandler<UserCreatedDomainEvent>
{
	private readonly IIntegrationEventPublisher<UserAccessModuleId> _publisher;

	public UserCreatedEventHandler(IIntegrationEventPublisher<UserAccessModuleId> publisher)
	{
		_publisher = publisher;
	}

	public Task Handle(UserCreatedDomainEvent domainEvent, CancellationToken ct)
	{
		var userCrated = new UserCreatedIntegrationEvent
		{
			UserId = domainEvent.User.Id,
			Email = domainEvent.User.Email,
			Name = domainEvent.User.Name
		};

		_publisher.Publish(userCrated);

		var emailCreated = new EmailCreatedIntegrationEvent
		{
			Email = domainEvent.User.Email,
			Subject = "Activation Email",
			Message = "Activate account!"
		};

		_publisher.Publish(emailCreated);

		return Task.CompletedTask;
	}
}
