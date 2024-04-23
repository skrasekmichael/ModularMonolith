using RailwayResult;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Application;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Contracts.Teams.ChangeNickname;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;

namespace TeamUp.TeamManagement.Application.Teams;

internal sealed class ChangeNicknameCommandHandler : ICommandHandler<ChangeNicknameCommand>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;

	public ChangeNicknameCommandHandler(ITeamRepository teamRepository, IUnitOfWork<TeamManagementModuleId> unitOfWork)
	{
		_teamRepository = teamRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(ChangeNicknameCommand command, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(command.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.ChangeNickname(command.InitiatorId, command.Nickname))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
