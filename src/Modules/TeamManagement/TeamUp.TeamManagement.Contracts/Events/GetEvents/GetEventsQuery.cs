using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Events.GetEvents;

public sealed record GetEventsQuery : IQuery<Collection<EventSlimResponse>>
{
	public required UserId InitiatorId { get; init; }
	public required TeamId TeamId { get; init; }
	public DateTime? FromUtc { get; init; } = null;
}
