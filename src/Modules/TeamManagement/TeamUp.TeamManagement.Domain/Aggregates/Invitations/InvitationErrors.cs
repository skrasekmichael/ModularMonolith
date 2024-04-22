using RailwayResult.Errors;

namespace TeamUp.TeamManagement.Domain.Aggregates.Invitations;

public static class InvitationErrors
{
	public static readonly AuthorizationError UnauthorizedToAcceptInvitation = new("TeamManagement.Invitations.Authorization.Accept", "Not allowed to accept this invitation.");

	public static readonly NotFoundError InvitationNotFound = new("TeamManagement.Invitations.NotFound", "Invitation not found.");

	public static readonly DomainError InvitationExpired = new("TeamManagement.Invitations.Domain.Expired", "Invitation has expired.");

	public static readonly ConflictError UserIsAlreadyInvited = new("TeamManagement.Invitations.Conflict.InvitedUser", "User has been already invited to this team.");
}

