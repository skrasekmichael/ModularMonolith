using System.ComponentModel.DataAnnotations;

using TeamUp.TeamManagement.Contracts.Teams;

namespace TeamUp.TeamManagement.Contracts.Invitations.InviteUser;

public sealed class InviteUserRequest
{
	public required TeamId TeamId { get; init; }

	[DataType(DataType.EmailAddress)]
	public required string Email { get; init; }
}
