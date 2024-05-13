using TeamUp.TeamManagement.Contracts.Teams;

namespace TeamUp.Contracts.Teams;

public sealed class TeamResponse
{
	public required string Name { get; init; }
	public required IReadOnlyList<TeamMemberResponse> Members { get; init; }
	public required IReadOnlyList<EventTypeResponse> EventTypes { get; init; }
}
