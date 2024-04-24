using RailwayResult;

using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Domain.Aggregates.Events;

public interface IEventDomainService
{
	public Task<Result<EventId>> CreateEventAsync(
		UserId initiatorId,
		TeamId teamId,
		EventTypeId eventTypeId,
		DateTime fromUtc,
		DateTime toUtc,
		string description,
		TimeSpan meetTime,
		TimeSpan replyClosingTimeBeforeMeetTime,
		CancellationToken ct = default);

	public Task<Result> DeleteEventAsync(UserId initiatorId, TeamId teamId, EventId eventId, CancellationToken ct = default);
}
