using TeamUp.Common.Domain;

namespace TeamUp.TeamManagement.Domain.Aggregates.Events.DomainEvents;

public sealed record EventResponseCreatedDomainEvent(EventResponse Response) : IDomainEvent;
