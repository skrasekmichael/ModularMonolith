using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Events.GetEvent;

public sealed record GetEventQuery : IQuery<EventResponse>
{
	public required UserId InitiatorId { get; init; }
	public required TeamId TeamId { get; init; }
	public required EventId EventId { get; init; }
}
