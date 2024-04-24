using RailwayResult;

using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Domain.Aggregates.Invitations;

public interface IInvitationDomainService
{
	public Task<Result> InviteUserAsync(UserId initiatorId, TeamId teamId, string email, CancellationToken ct = default);
	public Task<Result> RemoveInvitationAsync(UserId initiatorId, InvitationId invitationId, CancellationToken ct = default);
	public Task<Result> AcceptInvitationAsync(UserId initiatorId, InvitationId invitationId, CancellationToken ct = default);
}
