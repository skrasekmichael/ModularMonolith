using TeamUp.Common.Contracts;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Invitations.GetMyInvitations;

public sealed record GetMyInvitationsQuery : IQuery<Collection<InvitationResponse>>
{
	public required UserId InitiatorId { get; init; }
}
