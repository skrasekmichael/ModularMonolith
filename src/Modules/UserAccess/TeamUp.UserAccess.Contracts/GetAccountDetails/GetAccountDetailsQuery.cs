using TeamUp.Application.Abstractions;

namespace TeamUp.UserAccess.Contracts.GetAccountDetails;

public sealed record GetAccountDetailsQuery : IQuery<AccountResponse>
{
	public required UserId UserId { get; init; }
}
