using FluentAssertions.Extensions;

using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.Tests.Common.Extensions;

namespace TeamUp.Tests.Common.DataGenerators.TeamManagement;

public static class EventGeneratorExtensions
{
	public static EventGenerator WithEventType(this EventGenerator generator, EventTypeId eventTypeId) => generator.RuleFor(x => x.EventTypeId, eventTypeId);

	public static EventGenerator WithRandomEventResponses(this EventGenerator generator, IEnumerable<TeamMember> members)
	{
		return generator
			.RuleFor(EventGenerators.EVENT_RESPONSES_FIELD, (f, e) => members
				.Select(member => EventGenerators.Response
					.RuleForBackingField(er => er.EventId, e.Id)
					.RuleForBackingField(er => er.TeamMemberId, member.Id)
					.RuleFor(er => er.TimeStampUtc, f => f.Date
						.Between(DateTime.UtcNow.AddDays(-2), e.FromUtc - e.MeetTime - e.ReplyClosingTimeBeforeMeetTime)
						.DropMicroSeconds()
						.AsUtc())
					.RuleFor(er => er.ReplyType, f => f.PickRandom(EventGenerators.ReplyTypes))
					.RuleFor(er => er.Message, (f, er) => er.ReplyType == ReplyType.Yes ? string.Empty : f.Random.Text(1, EventConstants.EVENT_REPLY_MESSAGE_MAX_SIZE))
					.Generate())
				.ToList());
	}

	public static EventGenerator WithEventResponses(this EventGenerator generator, List<(TeamMember Member, ReplyType Type)> responses)
	{
		return generator
			.RuleFor(EventGenerators.EVENT_RESPONSES_FIELD, (f, e) => responses
				.Select(response => EventGenerators.Response
					.RuleForBackingField(er => er.EventId, e.Id)
					.RuleForBackingField(er => er.TeamMemberId, response.Member.Id)
					.RuleFor(er => er.TimeStampUtc, f => f.Date
						.Between(DateTime.UtcNow.AddDays(-2), e.FromUtc - e.MeetTime - e.ReplyClosingTimeBeforeMeetTime)
						.DropMicroSeconds()
						.AsUtc())
					.RuleFor(er => er.ReplyType, response.Type)
					.RuleFor(er => er.Message, (f, er) => er.ReplyType == ReplyType.Yes ? string.Empty : f.Random.Text(1, EventConstants.EVENT_REPLY_MESSAGE_MAX_SIZE))
					.Generate())
				.ToList());
	}

	public static EventGenerator WithStatus(this EventGenerator generator, EventStatus status) => generator.RuleFor(e => e.Status, status);

	public static EventGenerator ForTeam(this EventGenerator generator, TeamId teamId) => generator.RuleForBackingField(e => e.TeamId, teamId);
}
