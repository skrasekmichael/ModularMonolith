using TeamUp.Common.Domain;

namespace TeamUp.TeamManagement.Domain.Aggregates.Events.DomainEvents;

public sealed record EventStatusChangedDomainEvent(Event Event) : IDomainEvent;
