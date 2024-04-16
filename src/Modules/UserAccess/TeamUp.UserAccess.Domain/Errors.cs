using TeamUp.UserAccess.Contracts;

namespace TeamUp.UserAccess.Domain;

public static class Errors
{
	public static readonly ValidationError UserNameMinSize = new("UsersAccess.Validation.NameMinSize", $"Name must be atleast {Constants.USERNAME_MIN_SIZE} characters long.");
	public static readonly ValidationError UserNameMaxSize = new("UsersAccess.Validation.NameMaxSize", $"Name must be shorter than {Constants.USERNAME_MAX_SIZE} characters.");

	public static readonly NotFoundError UserNotFound = new("UsersAccess.NotFound", "User not found.");
	public static readonly NotFoundError AccountNotFound = new("UsersAccess.NotFound.Account", "Account not found.");

	public static readonly ConflictError ConflictingEmail = new("UsersAccess.Conflict.Email", "User with this email is already registered.");
}
