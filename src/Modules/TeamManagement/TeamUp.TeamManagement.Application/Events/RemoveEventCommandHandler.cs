using RailwayResult;

using TeamUp.Common.Application;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Contracts.Events.RemoveEvent;
using TeamUp.TeamManagement.Domain.Aggregates.Events;

namespace TeamUp.TeamManagement.Application.Events;

internal sealed class RemoveEventCommandHandler : ICommandHandler<RemoveEventCommand>
{
	private readonly IEventDomainService _eventDomainService;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;

	public RemoveEventCommandHandler(IEventDomainService eventDomainService, IUnitOfWork<TeamManagementModuleId> unitOfWork)
	{
		_eventDomainService = eventDomainService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(RemoveEventCommand command, CancellationToken ct)
	{
		return await _eventDomainService
			.DeleteEventAsync(command.InitiatorId, command.TeamId, command.EventId, ct)
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
