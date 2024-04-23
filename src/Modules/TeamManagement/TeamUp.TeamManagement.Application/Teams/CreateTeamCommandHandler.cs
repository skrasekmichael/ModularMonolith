using RailwayResult;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Contracts.Teams.CreateTeam;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Users;

namespace TeamUp.TeamManagement.Application.Teams;

internal sealed class CreateTeamCommandHandler : ICommandHandler<CreateTeamCommand, TeamId>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly IUserRepository _userRepository;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;

	public CreateTeamCommandHandler(ITeamRepository teamRepository, IDateTimeProvider dateTimeProvider, IUserRepository userRepository, IUnitOfWork<TeamManagementModuleId> unitOfWork)
	{
		_teamRepository = teamRepository;
		_dateTimeProvider = dateTimeProvider;
		_userRepository = userRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<TeamId>> Handle(CreateTeamCommand command, CancellationToken ct)
	{
		var user = await _userRepository.GetUserByIdAsync(command.OwnerId, ct);
		return await user
			.EnsureNotNull(UserErrors.UserNotFound)
			.Ensure(TeamRules.UserDoesNotOwnToManyTeams)
			.Then(user => Team.Create(command.Name, user, _dateTimeProvider))
			.Tap(_teamRepository.AddTeam)
			.Then(team => team.Id)
			.TapAsync(_ => _unitOfWork.SaveChangesAsync(ct));
	}
}
