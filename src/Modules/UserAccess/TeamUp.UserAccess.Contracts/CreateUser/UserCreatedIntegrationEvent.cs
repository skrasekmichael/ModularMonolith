using TeamUp.Domain.Abstractions;

namespace TeamUp.UserAccess.Contracts.CreateUser;

public sealed record UserCreatedIntegrationEvent : IIntegrationEvent
{
	public required UserId UserId { get; init; }
	public required string Email { get; init; }
	public required string Name { get; init; }
}
