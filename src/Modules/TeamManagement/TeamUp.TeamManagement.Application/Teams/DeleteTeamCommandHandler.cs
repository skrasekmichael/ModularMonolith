using RailwayResult;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Application;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Contracts.Teams.DeleteTeam;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;

namespace TeamUp.TeamManagement.Application.Teams;

internal sealed class DeleteTeamCommandHandler : ICommandHandler<DeleteTeamCommand>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;

	public DeleteTeamCommandHandler(ITeamRepository teamDomainService, IUnitOfWork<TeamManagementModuleId> unitOfWork)
	{
		_teamRepository = teamDomainService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(DeleteTeamCommand command, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(command.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.Delete(command.InitiatorId))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
