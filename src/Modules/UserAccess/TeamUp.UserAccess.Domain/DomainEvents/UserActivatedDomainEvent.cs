using TeamUp.Common.Domain;

namespace TeamUp.UserAccess.Domain.DomainEvents;

public sealed record UserActivatedDomainEvent(User User) : IDomainEvent;
