using System.ComponentModel.DataAnnotations;

namespace TeamUp.UserAccess.Contracts.Login;

public sealed record LoginRequest
{
	[DataType(DataType.EmailAddress)]
	public required string Email { get; init; }

	[DataType(DataType.Password)]
	public required string Password { get; init; }
}
