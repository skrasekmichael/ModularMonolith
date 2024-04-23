using RailwayResult;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Application;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Contracts.Teams.CreateEventType;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;

namespace TeamUp.TeamManagement.Application.Teams;

internal sealed class CreateEventTypeCommandHandler : ICommandHandler<CreateEventTypeCommand, EventTypeId>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;

	public CreateEventTypeCommandHandler(ITeamRepository teamRepository, IUnitOfWork<TeamManagementModuleId> unitOfWork)
	{
		_teamRepository = teamRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<EventTypeId>> Handle(CreateEventTypeCommand command, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(command.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.CreateEventType(command.InitiatorId, command.Name, command.Description))
			.TapAsync(_ => _unitOfWork.SaveChangesAsync(ct));
	}
}
