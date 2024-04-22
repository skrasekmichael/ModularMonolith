using TeamUp.Common.Contracts;

namespace TeamUp.UserAccess.Contracts.DeleteAccount;

public sealed record UserDeletedIntegrationEvent : IIntegrationEvent
{
	public required UserId UserId { get; init; }
}
