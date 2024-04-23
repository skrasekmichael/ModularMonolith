using System.ComponentModel.DataAnnotations;

namespace TeamUp.TeamManagement.Contracts.Teams.CreateEventType;

public sealed record UpsertEventTypeRequest
{
	[DataType(DataType.Text)]
	public required string Name { get; init; }

	[DataType(DataType.Text)]
	public required string Description { get; init; }
}
