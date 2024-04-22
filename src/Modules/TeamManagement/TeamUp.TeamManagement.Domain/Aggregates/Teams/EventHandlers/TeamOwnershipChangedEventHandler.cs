using Microsoft.Extensions.Logging;

using TeamUp.Common.Contracts;
using TeamUp.Common.Domain;
using TeamUp.TeamManagement.Domain.Aggregates.Teams.DomainEvents;
using TeamUp.TeamManagement.Domain.Aggregates.Users;

namespace TeamUp.TeamManagement.Domain.Aggregates.Teams.EventHandlers;

internal sealed class TeamOwnershipChangedEventHandler : IDomainEventHandler<TeamOwnershipChangedDomainEvent>
{
	private readonly IUserRepository _userRepository;
	private readonly ILogger<TeamOwnershipChangedEventHandler> _logger;

	public TeamOwnershipChangedEventHandler(IUserRepository userRepository, ILogger<TeamOwnershipChangedEventHandler> logger)
	{
		_userRepository = userRepository;
		_logger = logger;
	}

	public async Task Handle(TeamOwnershipChangedDomainEvent domainEvent, CancellationToken ct)
	{
		var oldOwner = await _userRepository.GetUserByIdAsync(domainEvent.OldOwner.UserId, ct);
		if (oldOwner is null)
		{
			var exception = new InternalException($"Old owner ({domainEvent.OldOwner.UserId}) has not been found.");
			_logger.LogCritical(exception, "Error occurred when changing team ({tramId}) ownership.", domainEvent.OldOwner.TeamId);
			throw exception;
		}

		var newOwner = await _userRepository.GetUserByIdAsync(domainEvent.NewOwner.UserId, ct);
		if (newOwner is null)
		{
			var exception = new InternalException($"New owner ({domainEvent.NewOwner.UserId}) has not been found.");
			_logger.LogCritical(exception, "Error occurred when changing team ({tramId}) ownership.", domainEvent.NewOwner.TeamId);
			throw exception;
		}

		oldOwner.DecreaseNumberOfOwningTeams();
		newOwner.IncreaseNumberOfOwningTeams();
	}
}
