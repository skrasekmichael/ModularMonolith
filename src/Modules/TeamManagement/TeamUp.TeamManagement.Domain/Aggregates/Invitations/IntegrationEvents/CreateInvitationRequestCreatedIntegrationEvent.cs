using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Domain.Aggregates.Invitations.IntegrationEvents;

public sealed record CreateInvitationRequestCreatedIntegrationEvent : IIntegrationEvent
{
	public required UserId UserId { get; init; }
	public required TeamId TeamId { get; init; }
}
