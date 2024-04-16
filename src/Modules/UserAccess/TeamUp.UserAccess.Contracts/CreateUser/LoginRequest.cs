using System.ComponentModel.DataAnnotations;

namespace TeamUp.UserAccess.Contracts.CreateUser;

public sealed record RegisterUserRequest
{
	[DataType(DataType.Text)]
	public required string Name { get; init; }

	[DataType(DataType.EmailAddress)]
	public required string Email { get; init; }

	[DataType(DataType.Password)]
	public required string Password { get; init; }
}
