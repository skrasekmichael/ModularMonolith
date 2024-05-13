namespace TeamUp.TeamManagement.Contracts.Teams;

public sealed class EventTypeResponse
{
	public required EventTypeId Id { get; init; }
	public required string Name { get; init; }
	public required string Description { get; init; }
}
