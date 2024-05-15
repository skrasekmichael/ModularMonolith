namespace TeamUp.TeamManagement.Contracts.Teams;

public sealed class TeamSlimResponse
{
	public required TeamId TeamId { get; init; }
	public required string Name { get; init; }
	public required int NumberOfTeamMembers { get; init; }
}
