using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Teams;

public sealed class TeamMemberResponse
{
	public required TeamMemberId Id { get; init; }
	public required UserId UserId { get; init; }
	public required string Nickname { get; init; }
	public required TeamRole Role { get; init; }
}
