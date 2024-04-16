namespace TeamUp.UserAccess.Application;

public static class AuthenticationErrors
{
	public static readonly AuthenticationError InvalidCredentials = new("UserAccess.Authentication.InvalidCredentials", "Invalid Credentials.");
	public static readonly AuthenticationError NotActivatedAccount = new("UserAccess.Authentication.NotActivatedAccount", "Account is not activated.");
}
