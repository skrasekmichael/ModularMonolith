using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Domain.Aggregates.Invitations;

public interface IInvitationRepository
{
	public void AddInvitation(Invitation invitation);
	public void RemoveInvitation(Invitation invitation);
	public Task<Invitation?> GetInvitationByIdAsync(InvitationId invitationId, CancellationToken ct = default);
	public Task<bool> ExistsInvitationForUserToTeamAsync(UserId userId, TeamId teamId, CancellationToken ct = default);
}
