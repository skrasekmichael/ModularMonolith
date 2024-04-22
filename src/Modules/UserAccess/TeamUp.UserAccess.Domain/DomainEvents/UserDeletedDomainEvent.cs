using TeamUp.Common.Domain;

namespace TeamUp.UserAccess.Domain.DomainEvents;

internal sealed record UserDeletedDomainEvent(User User) : IDomainEvent;
