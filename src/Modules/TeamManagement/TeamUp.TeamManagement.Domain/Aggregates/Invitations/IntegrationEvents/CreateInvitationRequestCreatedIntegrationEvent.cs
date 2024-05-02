using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts.Teams;

namespace TeamUp.TeamManagement.Domain.Aggregates.Invitations.IntegrationEvents;

public sealed record CreateInvitationRequestCreatedIntegrationEvent : IIntegrationEvent
{
	public required string Email { get; init; }
	public required TeamId TeamId { get; init; }
}
