using TeamUp.Common.Domain;

namespace TeamUp.TeamManagement.Domain.Aggregates.Teams.DomainEvents;

public sealed record TeamOwnershipChangedDomainEvent(TeamMember OldOwner, TeamMember NewOwner) : IDomainEvent;
