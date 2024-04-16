namespace TeamUp.UserAccess.Contracts.GetAccountDetails;

public sealed class AccountResponse
{
	public required string Email { get; set; }
	public required string Name { get; set; }
	public required UserState State { get; set; }
}
