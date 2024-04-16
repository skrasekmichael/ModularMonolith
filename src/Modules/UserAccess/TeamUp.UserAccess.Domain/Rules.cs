using TeamUp.UserAccess.Contracts;

namespace TeamUp.UserAccess.Domain;

public static class Rules
{
	static readonly Rule<string> UserNameMinSizeRule = name => name.Length >= Constants.USERNAME_MIN_SIZE;
	static readonly Rule<string> UserNameMaxSizeRule = name => name.Length <= Constants.USERNAME_MAX_SIZE;

	public static readonly RuleWithError<string> UserNameMinSize = new(UserNameMinSizeRule, Errors.UserNameMinSize);
	public static readonly RuleWithError<string> UserNameMaxSize = new(UserNameMaxSizeRule, Errors.UserNameMaxSize);
}
