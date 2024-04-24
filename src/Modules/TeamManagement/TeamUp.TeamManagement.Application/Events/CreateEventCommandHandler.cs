using RailwayResult;

using TeamUp.Common.Application;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Events.CreateEvent;
using TeamUp.TeamManagement.Domain.Aggregates.Events;

namespace TeamUp.TeamManagement.Application.Events;

internal sealed class CreateEventCommandHandler : ICommandHandler<CreateEventCommand, EventId>
{
	private readonly IEventDomainService _eventDomainService;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;

	public CreateEventCommandHandler(IEventDomainService eventDomainService, IUnitOfWork<TeamManagementModuleId> unitOfWork)
	{
		_eventDomainService = eventDomainService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<EventId>> Handle(CreateEventCommand command, CancellationToken ct)
	{
		return await _eventDomainService
			.CreateEventAsync(
				initiatorId: command.InitiatorId,
				teamId: command.TeamId,
				eventTypeId: command.EventTypeId,
				fromUtc: command.FromUtc,
				toUtc: command.ToUtc,
				description: command.Description,
				meetTime: command.MeetTime,
				replyClosingTimeBeforeMeetTime: command.ReplyClosingTimeBeforeMeetTime,
				ct: ct)
			.TapAsync(_ => _unitOfWork.SaveChangesAsync(ct));
	}
}
