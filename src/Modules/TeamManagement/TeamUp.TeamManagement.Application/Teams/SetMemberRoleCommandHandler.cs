using RailwayResult;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Application;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Contracts.Teams.SetMemberRole;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;

namespace TeamUp.TeamManagement.Application.Teams;

internal sealed class SetMemberRoleCommandHandler : ICommandHandler<SetMemberRoleCommand>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;

	public SetMemberRoleCommandHandler(ITeamRepository teamRepository, IUnitOfWork<TeamManagementModuleId> unitOfWork)
	{
		_teamRepository = teamRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(SetMemberRoleCommand command, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(command.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.SetMemberRole(command.InitiatorId, command.MemberId, command.Role))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
