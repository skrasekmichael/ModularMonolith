using TeamUp.Common.Contracts;

namespace TeamUp.UserAccess.Contracts.GetAccountDetails;

public sealed record GetUserDetailsQuery : IQuery<AccountResponse>
{
	public required UserId UserId { get; init; }
}
