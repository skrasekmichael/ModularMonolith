using System.ComponentModel.DataAnnotations;

namespace TeamUp.TeamManagement.Contracts.Teams.SetTeamName;

public sealed class UpdateTeamNameRequest
{
	[DataType(DataType.Text)]
	public required string Name { get; init; }
}
