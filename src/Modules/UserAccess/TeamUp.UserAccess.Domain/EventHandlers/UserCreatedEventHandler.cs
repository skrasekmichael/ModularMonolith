using TeamUp.Common.Domain;
using TeamUp.Notifications.Contracts;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.CreateUser;
using TeamUp.UserAccess.Domain.DomainEvents;

namespace TeamUp.UserAccess.Domain.EventHandlers;

internal sealed class UserCreatedEventHandler : IDomainEventHandler<UserCreatedDomainEvent>
{
	private readonly IIntegrationEventPublisher<UserAccessModuleId> _publisher;
	private readonly IClientUrlGenerator _urlGenerator;

	public UserCreatedEventHandler(IIntegrationEventPublisher<UserAccessModuleId> publisher, IClientUrlGenerator urlGenerator)
	{
		_publisher = publisher;
		_urlGenerator = urlGenerator;
	}

	public Task Handle(UserCreatedDomainEvent domainEvent, CancellationToken ct)
	{
		var userCrated = new UserCreatedIntegrationEvent
		{
			UserId = domainEvent.User.Id,
			Email = domainEvent.User.Email,
			Name = domainEvent.User.Name,
		};

		_publisher.Publish(userCrated);

		if (domainEvent.User.State == UserState.NotActivated)
		{
			var emailCreated = new EmailCreatedIntegrationEvent
			{
				Email = domainEvent.User.Email,
				Subject = "Successful Registration",
				Message = $"You need to activate at your account at {_urlGenerator.GetActivationUrl(domainEvent.User.Id)} to finalize your registration."
			};

			_publisher.Publish(emailCreated);
		}
		else if (domainEvent.User.State == UserState.Generated)
		{
			var emailCreated = new EmailCreatedIntegrationEvent
			{
				Email = domainEvent.User.Email,
				Subject = "Account has been created",
				Message = $"You need to finalize your registration at {_urlGenerator.GetCompleteAccountRegistrationUrl(domainEvent.User.Id)}."
			};

			_publisher.Publish(emailCreated);
		}

		return Task.CompletedTask;
	}
}
