using TeamUp.Common.Domain;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.DeleteAccount;
using TeamUp.UserAccess.Domain.DomainEvents;

namespace TeamUp.UserAccess.Domain.EventHandlers;

internal sealed class UserDeletedEventHandler : IDomainEventHandler<UserDeletedDomainEvent>
{
	private readonly IUserRepository _userRepository;
	private readonly IIntegrationEventPublisher<UserAccessModuleId> _integrationEventPublisher;

	public UserDeletedEventHandler(IUserRepository userRepository, IIntegrationEventPublisher<UserAccessModuleId> integrationEventPublisher)
	{
		_userRepository = userRepository;
		_integrationEventPublisher = integrationEventPublisher;
	}

	public Task Handle(UserDeletedDomainEvent domainEvent, CancellationToken ct)
	{
		var message = new UserDeletedIntegrationEvent
		{
			UserId = domainEvent.User.Id
		};

		_integrationEventPublisher.Publish(message);
		_userRepository.RemoveUser(domainEvent.User);
		return Task.CompletedTask;
	}
}
