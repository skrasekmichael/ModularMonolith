using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Domain.Aggregates.Teams;

public interface ITeamRepository
{
	public Task<Team?> GetTeamByIdAsync(TeamId teamId, CancellationToken ct = default);
	public Task<List<Team>> GetTeamsByUserIdAsync(UserId userId, CancellationToken ct = default);
	public void AddTeam(Team team);
	public void RemoveTeam(Team team);
}
