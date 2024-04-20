using TeamUp.Common.Contracts;

namespace TeamUp.Notifications.Contracts;

public sealed record EmailCreatedIntegrationEvent : IIntegrationEvent
{
	public required string Email { get; init; }
	public required string Subject { get; init; }
	public required string Message { get; init; }
}
