using Microsoft.EntityFrameworkCore;

using RailwayResult;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Application;
using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Events.GetEvent;
using TeamUp.TeamManagement.Domain.Aggregates.Events;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;

using EventResponse = TeamUp.TeamManagement.Contracts.Events.EventResponse;

namespace TeamUp.TeamManagement.Application.Events;

internal sealed class GetEventQueryHandler : IQueryHandler<GetEventQuery, EventResponse>
{
	private readonly ITeamManagementQueryContext _appQueryContext;

	public GetEventQueryHandler(ITeamManagementQueryContext appQueryContext)
	{
		_appQueryContext = appQueryContext;
	}

	public async Task<Result<EventResponse>> Handle(GetEventQuery query, CancellationToken ct)
	{
		var team = await _appQueryContext.Teams
			.AsSplitQuery()
			.Include(team => team.Members)
			.Select(team => new
			{
				team.Id,
				Event = _appQueryContext.Events
					.AsSplitQuery()
					.Where(e => e.Id == query.EventId && e.TeamId == team.Id)
					.Select(e => new EventResponse
					{
						Description = e.Description,
						EventTypeId = e.EventTypeId,
						EventType = team.EventTypes.First(et => et.Id == e.EventTypeId).Name,
						FromUtc = e.FromUtc,
						ToUtc = e.ToUtc,
						MeetTime = e.MeetTime,
						ReplyClosingTimeBeforeMeetTime = e.ReplyClosingTimeBeforeMeetTime,
						Status = e.Status,
						EventResponses = e.EventResponses.Select(er => new EventResponseResponse
						{
							Message = er.Message,
							TeamMemberId = er.TeamMemberId,
							TeamMemberNickname = team.Members.First(member => member.Id == er.TeamMemberId).Nickname,
							TimeStampUtc = er.TimeStampUtc,
							Type = er.ReplyType
						}).ToList()
					})
					.FirstOrDefault(),
				Initiator = team.Members
					.Select(member => member.UserId)
					.FirstOrDefault(id => id == query.InitiatorId)
			})
			.FirstOrDefaultAsync(team => team.Id == query.TeamId, ct);

		return team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.EnsureNotNull(team => team.Initiator, TeamErrors.NotMemberOfTeam)
			.EnsureNotNull(team => team.Event, EventErrors.EventNotFound)
			.Then(team => team.Event!);
	}
}
