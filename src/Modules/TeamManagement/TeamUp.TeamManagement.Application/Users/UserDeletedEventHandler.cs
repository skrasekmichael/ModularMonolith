using RailwayResult;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Application;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Teams.DomainEvents;
using TeamUp.TeamManagement.Domain.Aggregates.Users;
using TeamUp.UserAccess.Contracts.DeleteAccount;

namespace TeamUp.TeamManagement.Application.Users;

internal class UserDeletedEventHandler : IIntegrationEventHandler<UserDeletedIntegrationEvent>
{
	private readonly IUserRepository _userRepository;
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;

	public UserDeletedEventHandler(IUserRepository userRepository, ITeamRepository teamRepository, IUnitOfWork<TeamManagementModuleId> unitOfWork)
	{
		_userRepository = userRepository;
		_teamRepository = teamRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(UserDeletedIntegrationEvent integrationEvent, CancellationToken ct)
	{
		var teams = await _teamRepository.GetTeamsByUserIdAsync(integrationEvent.UserId, ct);

		foreach (var team in teams)
		{
			team.GetTeamMemberByUserId(integrationEvent.UserId)
				.Ensure(TeamRules.MemberCanChangeOwnership)
				.Tap(initiator =>
				{
					if (team.Members.Count == 1)
					{
						//remove team if user that is being removed is the only member
						_teamRepository.RemoveTeam(team);
					}
					else
					{
						//change ownership when removing user that is owner of the team
						var newOwner = team.GetHighestNonOwnerTeamMember()!;
						initiator.UpdateRole(TeamRole.Admin);
						newOwner.UpdateRole(TeamRole.Owner);
						team.AddDomainEvent(new TeamOwnershipChangedDomainEvent(initiator, newOwner));
					}
				});

			//db will cascade delete member, but number of members needs to be updated manually
			team.DecreaseNumberOfMembers();
		}

		//delete user
		var user = await _userRepository.GetUserByIdAsync(integrationEvent.UserId, ct);
		if (user is not null)
		{
			_userRepository.RemoveUser(user);
		}

		return await _unitOfWork.SaveChangesAsync(ct);
	}
}
