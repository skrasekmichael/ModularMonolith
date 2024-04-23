using System.ComponentModel.DataAnnotations;

namespace TeamUp.TeamManagement.Contracts.Teams.ChangeNickname;

public sealed class ChangeNicknameRequest
{
	[DataType(DataType.Text)]
	public required string Nickname { get; init; }
}
