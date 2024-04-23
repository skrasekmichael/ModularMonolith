using System.ComponentModel.DataAnnotations;

namespace TeamUp.TeamManagement.Contracts.Teams.CreateTeam;

public sealed record CreateTeamRequest
{
	[DataType(DataType.Text)]
	public required string Name { get; init; }
}
