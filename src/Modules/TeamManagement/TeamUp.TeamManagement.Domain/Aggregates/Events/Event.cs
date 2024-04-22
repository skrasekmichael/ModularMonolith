using RailwayResult;
using RailwayResult.Errors;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Contracts;
using TeamUp.Common.Domain;
using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Events.DomainEvents;

namespace TeamUp.TeamManagement.Domain.Aggregates.Events;

public sealed class Event : AggregateRoot<Event, EventId>
{
	private readonly List<EventResponse> _eventResponses = [];

	public EventTypeId EventTypeId { get; private set; }
	public TeamId TeamId { get; }

	public DateTime FromUtc { get; private set; }
	public DateTime ToUtc { get; private set; }
	public string Description { get; private set; }
	public EventStatus Status { get; private set; }
	public TimeSpan MeetTime { get; private set; }
	public TimeSpan ReplyClosingTimeBeforeMeetTime { get; private set; }
	public IReadOnlyList<EventResponse> EventResponses => _eventResponses.AsReadOnly();

#pragma warning disable CS8618 // EF Core constructor
	private Event() : base() { }
#pragma warning restore CS8618

	internal Event(
		EventId id,
		EventTypeId eventTypeId,
		TeamId teamId,
		DateTime fromUtc,
		DateTime toUtc,
		string description,
		EventStatus status,
		TimeSpan meetTime,
		TimeSpan replyClosingTimeBeforeMeetTime) : base(id)
	{
		EventTypeId = eventTypeId;
		TeamId = teamId;
		FromUtc = fromUtc;
		ToUtc = toUtc;
		Description = description;
		Status = status;
		MeetTime = meetTime;
		ReplyClosingTimeBeforeMeetTime = replyClosingTimeBeforeMeetTime;
	}

	public Result SetMemberResponse(IDateTimeProvider dateTimeProvider, TeamMemberId memberId, EventReply reply)
	{
		static bool TimeForResponsesHasNotExpired(IDateTimeProvider dateTimeProvider, DateTime responseCloseTime) => dateTimeProvider.UtcNow < responseCloseTime;

		return Status
			.Ensure(status => status.IsOpenForResponses(), EventErrors.NotOpenForResponses)
			.Then(_ => GetResponseCloseTime())
			.Ensure(responseCloseTime => TimeForResponsesHasNotExpired(dateTimeProvider, responseCloseTime), EventErrors.TimeForResponsesExpired)
			.Then(_ => _eventResponses.Find(er => er.TeamMemberId == memberId))
			.Tap(response =>
			{
				if (response is null)
					_eventResponses.Add(EventResponse.Create(dateTimeProvider, memberId, Id, reply));
				else
					response.UpdateReply(dateTimeProvider, reply);
			})
			.ToResult();
	}

	public void UpdateStatus(EventStatus status)
	{
		if (Status == status)
			return;

		Status = status;
		AddDomainEvent(new EventStatusChangedDomainEvent(this));
	}

	private DateTime GetResponseCloseTime() => FromUtc - MeetTime - ReplyClosingTimeBeforeMeetTime;

	public static Result<Event> Create(
		EventTypeId eventTypeId,
		TeamId teamId,
		DateTime fromUtc,
		DateTime toUtc,
		string description,
		TimeSpan meetTime,
		TimeSpan replyClosingTimeBeforeMeetTime,
		IDateTimeProvider dateTimeProvider)
	{
		return fromUtc
			.Ensure(from => from < toUtc, EventErrors.CannotEndBeforeStart)
			.Ensure(from => from > dateTimeProvider.DateTimeOffsetUtcNow, EventErrors.CannotStartInPast)
			.Ensure<DateTime, ValidationError>(_ => description.Length <= EventConstants.EVENT_DESCRIPTION_MAX_SIZE, EventErrors.EventDescriptionMaxSize)
			.Then(_ => new Event(
				EventId.New(),
				eventTypeId,
				teamId,
				fromUtc,
				toUtc,
				description,
				EventStatus.Open,
				meetTime,
				replyClosingTimeBeforeMeetTime
			));
	}
}
