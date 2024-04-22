global using EventGenerator = Bogus.Faker<TeamUp.TeamManagement.Domain.Aggregates.Events.Event>;
global using EventResponseGenerator = Bogus.Faker<TeamUp.TeamManagement.Domain.Aggregates.Events.EventResponse>;

using FluentAssertions.Extensions;

using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.Tests.Common.Extensions;

namespace TeamUp.Tests.Common.DataGenerators.TeamManagement;

public sealed class EventGenerators : BaseGenerator
{
	public static readonly ReplyType[] ReplyTypes = [ReplyType.Yes, ReplyType.No, ReplyType.Maybe, ReplyType.Delay];

	internal const string EVENT_RESPONSES_FIELD = "_eventResponses";
	private static readonly PrivateBinder EventBinder = new(
		EVENT_RESPONSES_FIELD,
		nameof(TeamUp.TeamManagement.Domain.Aggregates.Events.Event.TeamId).GetBackingField()
	);

	private static readonly PrivateBinder EventResponseBinder = new(
		nameof(TeamUp.TeamManagement.Domain.Aggregates.Events.EventResponse.EventId).GetBackingField(),
		nameof(TeamUp.TeamManagement.Domain.Aggregates.Events.EventResponse.TeamMemberId).GetBackingField()
	);

	public static readonly EventGenerator Event = new EventGenerator(binder: EventBinder)
		.UsePrivateConstructor()
		.RuleFor(e => e.Id, f => EventId.New())
		.RuleFor(e => e.Description, f => f.Random.Text(1, EventConstants.EVENT_DESCRIPTION_MAX_SIZE))
		.RuleFor(e => e.MeetTime, f => f.Date.Timespan(TimeSpan.FromHours(24)).DropMicroSeconds())
		.RuleFor(e => e.ReplyClosingTimeBeforeMeetTime, f => f.Date.Timespan(TimeSpan.FromHours(24)).DropMicroSeconds())
		.RuleFor(e => e.FromUtc, f => f.Date.Between(DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddMonths(6)).DropMicroSeconds().AsUtc())
		.RuleFor(e => e.ToUtc, (f, e) => e.FromUtc.AddHours(f.Random.Int(1, 5)).AsUtc())
		.RuleFor(e => e.Status, EventStatus.Open);

	public static readonly EventResponseGenerator Response = new EventResponseGenerator(binder: EventResponseBinder)
		.UsePrivateConstructor()
		.RuleFor(er => er.Id, f => EventResponseId.New());
}
