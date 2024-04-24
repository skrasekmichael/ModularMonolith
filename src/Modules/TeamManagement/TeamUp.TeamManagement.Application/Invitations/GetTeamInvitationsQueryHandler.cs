using Microsoft.EntityFrameworkCore;

using RailwayResult;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Invitations.GetTeamInvitations;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;

namespace TeamUp.TeamManagement.Application.Invitations;

internal sealed class GetTeamInvitationsQueryHandler : IQueryHandler<GetTeamInvitationsQuery, Collection<TeamInvitationResponse>>
{
	private readonly ITeamManagementQueryContext _appQueryContext;

	public GetTeamInvitationsQueryHandler(ITeamManagementQueryContext appQueryContext)
	{
		_appQueryContext = appQueryContext;
	}

	public async Task<Result<Collection<TeamInvitationResponse>>> Handle(GetTeamInvitationsQuery query, CancellationToken ct)
	{
		var teamWithInitiator = await _appQueryContext.Teams
			.Select(team => new
			{
				team.Id,
				Initiaotor = team.Members
					.Select(member => new { member.UserId, member.Role })
					.FirstOrDefault(member => member.UserId == query.InitiatorId),
			})
			.FirstOrDefaultAsync(team => team.Id == query.TeamId, ct);

		return await teamWithInitiator
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.EnsureNotNull(team => team.Initiaotor, TeamErrors.NotMemberOfTeam)
			.Ensure(team => team.Initiaotor!.Role.CanInviteTeamMembers(), TeamErrors.UnauthorizedToReadInvitationList)
			.ThenAsync(team =>
			{
				return _appQueryContext.Invitations
					.Where(invitation => invitation.TeamId == query.TeamId)
					.Select(invitation => new TeamInvitationResponse
					{
						Id = invitation.Id,
						Email = _appQueryContext.Users
							.Select(user => new { user.Id, user.Email })
							.First(user => user.Id == invitation.RecipientId).Email,
						CreatedUtc = invitation.CreatedUtc
					})
					.ToListAsync(ct);
			})
			.Then(invitations => new Collection<TeamInvitationResponse>(invitations));
	}
}
