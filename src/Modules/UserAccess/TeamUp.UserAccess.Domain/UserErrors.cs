using TeamUp.UserAccess.Contracts;

namespace TeamUp.UserAccess.Domain;

public static class UserErrors
{
	public static readonly ValidationError UserNameMinSize = new("UsersAccess.Validation.NameMinSize", $"Name must be atleast {UserConstants.USERNAME_MIN_SIZE} characters long.");
	public static readonly ValidationError UserNameMaxSize = new("UsersAccess.Validation.NameMaxSize", $"Name must be shorter than {UserConstants.USERNAME_MAX_SIZE} characters.");

	public static readonly NotFoundError UserNotFound = new("UsersAccess.NotFound", "User not found.");

	public static readonly ConflictError ConflictingEmail = new("UsersAccess.Conflict.Email", "User with this email is already registered.");

	public static readonly DomainError CannotActivateGeneratedAccount = new("UsersAccess.Domain.ActivateGeneratedAccount", "Cannot activate generated account.");
	public static readonly DomainError AccountAlreadyActivated = new("UsersAccess.Domain.AccountAlreadyActivated", "Account is already activated.");
	public static readonly DomainError CannotCompleteRegistrationOfNonGeneratedAccount = new("UsersAccess.Domain.CompleteNonGeneratedAccount", "Cannot complete registration of non-generated account.");
}
