using Microsoft.EntityFrameworkCore;

using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Infrastructure.Persistence.Domain.Invitations;

internal sealed class InvitationRepository : IInvitationRepository
{
	private readonly TeamManagementDbContext _dbContext;

	public InvitationRepository(TeamManagementDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public void AddInvitation(Invitation invitation) => _dbContext.Invitations.Add(invitation);

	public void RemoveInvitation(Invitation invitation) => _dbContext.Invitations.Remove(invitation);

	public async Task<Invitation?> GetInvitationByIdAsync(InvitationId invitationId, CancellationToken ct = default)
	{
		return await _dbContext.Invitations.FindAsync([invitationId], ct);
	}

	public async Task<bool> ExistsInvitationForUserToTeamAsync(UserId userId, TeamId teamId, CancellationToken ct = default)
	{
		return await _dbContext.Invitations.AnyAsync(invitation => invitation.RecipientId == userId && invitation.TeamId == teamId, ct);
	}
}
