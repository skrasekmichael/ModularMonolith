namespace TeamUp.TeamManagement.Contracts.Invitations;

public sealed class TeamInvitationResponse
{
	public required InvitationId Id { get; init; }
	public required string Email { get; init; }
	public required DateTime CreatedUtc { get; init; }
}
