using Microsoft.EntityFrameworkCore;

using RailwayResult;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Invitations.GetMyInvitations;

namespace TeamUp.TeamManagement.Application.Invitations;

internal sealed class GetMyInvitationsQueryHandler : IQueryHandler<GetMyInvitationsQuery, Collection<InvitationResponse>>
{
	private readonly ITeamManagementQueryContext _appQueryContext;

	public GetMyInvitationsQueryHandler(ITeamManagementQueryContext appQueryContext)
	{
		_appQueryContext = appQueryContext;
	}

	public async Task<Result<Collection<InvitationResponse>>> Handle(GetMyInvitationsQuery query, CancellationToken ct)
	{
		var invitations = await _appQueryContext.Invitations
			.Where(invitation => invitation.RecipientId == query.InitiatorId)
			.Select(invitation => new InvitationResponse
			{
				Id = invitation.Id,
				TeamName = _appQueryContext.Teams.First(team => team.Id == invitation.TeamId).Name,
				CreatedUtc = invitation.CreatedUtc
			})
			.ToListAsync(ct);

		return new Collection<InvitationResponse>(invitations);
	}
}
