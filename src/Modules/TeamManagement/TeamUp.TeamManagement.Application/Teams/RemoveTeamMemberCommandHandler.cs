using RailwayResult;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Application;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Contracts.Teams.RemoveTeamMember;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;

namespace TeamUp.TeamManagement.Application.Teams;

internal sealed class RemoveTeamMemberCommandHandler : ICommandHandler<RemoveTeamMemberCommand>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;

	public RemoveTeamMemberCommandHandler(ITeamRepository teamRepository, IUnitOfWork<TeamManagementModuleId> unitOfWork)
	{
		_teamRepository = teamRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(RemoveTeamMemberCommand command, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(command.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.RemoveTeamMember(command.InitiatorId, command.MemberId))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
