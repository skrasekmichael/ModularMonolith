using RailwayResult;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Domain.Aggregates.Events;

internal sealed class EventDomainService : IEventDomainService
{
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly ITeamRepository _teamRepository;
	private readonly IEventRepository _eventRepository;

	public EventDomainService(IDateTimeProvider dateTimeProvider, ITeamRepository teamRepository, IEventRepository eventRepository)
	{
		_dateTimeProvider = dateTimeProvider;
		_teamRepository = teamRepository;
		_eventRepository = eventRepository;
	}

	public async Task<Result<EventId>> CreateEventAsync(
		UserId initiatorId,
		TeamId teamId,
		EventTypeId eventTypeId,
		DateTime fromUtc,
		DateTime toUtc,
		string description,
		TimeSpan meetTime,
		TimeSpan replyClosingTimeBeforeMeetTime,
		CancellationToken ct = default)
	{
		var team = await _teamRepository.GetTeamByIdAsync(teamId, ct);
		return team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Ensure(team => team.EventTypes.Any(type => type.Id == eventTypeId), TeamErrors.EventTypeNotFound)
			.And(team => team.GetTeamMemberByUserId(initiatorId))
			.Ensure((_, member) => member.Role.CanManipulateEvents(), TeamErrors.UnauthorizedToCreateEvents)
			.Then((team, _) => Event.Create(
				eventTypeId,
				team.Id,
				fromUtc,
				toUtc,
				description,
				meetTime,
				replyClosingTimeBeforeMeetTime,
				_dateTimeProvider
			))
			.Tap(_eventRepository.AddEvent)
			.Then(@event => @event.Id);
	}

	public async Task<Result> DeleteEventAsync(UserId initiatorId, TeamId teamId, EventId eventId, CancellationToken ct = default)
	{
		var team = await _teamRepository.GetTeamByIdAsync(teamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.GetTeamMemberByUserId(initiatorId))
			.Ensure(member => member.Role.CanManipulateEvents(), TeamErrors.UnauthorizedToDeleteEvents)
			.ThenAsync(_ => _eventRepository.GetEventByIdAsync(eventId, ct))
			.EnsureNotNull(EventErrors.EventNotFound)
			.Ensure(e => e.TeamId == teamId, EventErrors.EventNotFound)
			.Tap(_eventRepository.RemoveEvent)
			.ToResultAsync();
	}
}
