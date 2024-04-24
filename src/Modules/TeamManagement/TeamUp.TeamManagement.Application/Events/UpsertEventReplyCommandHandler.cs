using RailwayResult;
using RailwayResult.Errors;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Events.UpsertEventReply;
using TeamUp.TeamManagement.Domain.Aggregates.Events;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;

namespace TeamUp.TeamManagement.Application.Events;

internal sealed class UpsertEventReplyCommandHandler : ICommandHandler<UpsertEventReplyCommand>
{
	private readonly IEventRepository _eventRepository;
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;
	private readonly IDateTimeProvider _dateTimeProvider;

	public UpsertEventReplyCommandHandler(
		IEventRepository eventRepository,
		ITeamRepository teamRepository,
		IUnitOfWork<TeamManagementModuleId> unitOfWork,
		IDateTimeProvider dateTimeProvider)
	{
		_eventRepository = eventRepository;
		_teamRepository = teamRepository;
		_unitOfWork = unitOfWork;
		_dateTimeProvider = dateTimeProvider;
	}

	public async Task<Result> Handle(UpsertEventReplyCommand command, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(command.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.GetTeamMemberByUserId(command.InitiatorId))
			.AndAsync(_ => _eventRepository.GetEventByIdAsync(command.EventId, ct))
			.EnsureSecondNotNull(EventErrors.EventNotFound)
			.Ensure((_, @event) => @event.TeamId == command.TeamId, EventErrors.EventNotFound)
			.And((_, _) => MapRequestToReply(command))
			.Then((member, @event, reply) => @event.SetMemberResponse(_dateTimeProvider, member.Id, reply))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}

	private static Result<EventReply> MapRequestToReply(UpsertEventReplyCommand command) => command.ReplyType switch
	{
		ReplyType.Yes => EventReply.Yes(),
		ReplyType.Maybe => EventReply.Maybe(command.Message),
		ReplyType.Delay => EventReply.Delay(command.Message),
		ReplyType.No => EventReply.No(command.Message),
		_ => new InternalError("InternalErrors.MissingSwitchCase", $"{nameof(MapRequestToReply)} does not implement case for type [{command.ReplyType}]")
	};
}
