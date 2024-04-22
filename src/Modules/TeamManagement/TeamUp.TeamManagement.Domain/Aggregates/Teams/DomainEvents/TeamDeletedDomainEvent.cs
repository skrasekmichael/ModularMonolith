using TeamUp.Common.Domain;

namespace TeamUp.TeamManagement.Domain.Aggregates.Teams.DomainEvents;

public sealed record TeamDeletedDomainEvent(Team Team) : IDomainEvent;
