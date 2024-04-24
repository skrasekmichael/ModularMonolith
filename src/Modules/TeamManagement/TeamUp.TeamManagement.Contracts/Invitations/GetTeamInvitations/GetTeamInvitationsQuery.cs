using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Invitations.GetTeamInvitations;

public sealed record GetTeamInvitationsQuery : IQuery<Collection<TeamInvitationResponse>>
{
	public required UserId InitiatorId { get; init; }
	public required TeamId TeamId { get; init; }
}
