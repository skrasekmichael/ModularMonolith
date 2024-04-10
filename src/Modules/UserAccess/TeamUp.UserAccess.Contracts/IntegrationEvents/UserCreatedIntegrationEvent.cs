using TeamUp.Domain.Abstractions;

namespace TeamUp.UserAccess.Contracts.IntegrationEvents;

public sealed record UserCreatedIntegrationEvent(UserId UserId, string Email, string Name) : IIntegrationEvent;
