using TeamUp.Common.Domain;

namespace TeamUp.UserAccess.Domain.DomainEvents;

internal sealed record UserCreatedDomainEvent(User User) : IDomainEvent;
