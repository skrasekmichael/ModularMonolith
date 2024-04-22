using Microsoft.Extensions.Logging;

using TeamUp.Common.Contracts;
using TeamUp.Common.Domain;
using TeamUp.TeamManagement.Domain.Aggregates.Teams.DomainEvents;
using TeamUp.TeamManagement.Domain.Aggregates.Users;

namespace TeamUp.TeamManagement.Domain.Aggregates.Teams.EventHandlers;

internal sealed class TeamDeletedEventHandler : IDomainEventHandler<TeamDeletedDomainEvent>
{
	private readonly IUserRepository _userRepository;
	private readonly ITeamRepository _teamRepository;
	private readonly ILogger<TeamDeletedEventHandler> _logger;

	public TeamDeletedEventHandler(IUserRepository userRepository, ITeamRepository teamRepository, ILogger<TeamDeletedEventHandler> logger)
	{
		_userRepository = userRepository;
		_teamRepository = teamRepository;
		_logger = logger;
	}

	public async Task Handle(TeamDeletedDomainEvent domainEvent, CancellationToken ct)
	{
		var owner = domainEvent.Team.Members.First(member => member.Role.IsOwner());

		var user = await _userRepository.GetUserByIdAsync(owner.UserId, ct);
		if (user is null)
		{
			var exception = new InternalException($"Owner ({owner.UserId}) of deleted team ({domainEvent.Team.Id}) has not been found.");
			_logger.LogCritical(exception, "Error occurred when deleted team ({teamId}).", domainEvent.Team.Id);
			throw exception;
		}

		_teamRepository.RemoveTeam(domainEvent.Team);
		user.DecreaseNumberOfOwningTeams();
	}
}
