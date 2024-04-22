using Microsoft.EntityFrameworkCore;

using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Infrastructure.Persistence.Domain.Teams;

internal sealed class TeamRepository : ITeamRepository
{
	private readonly TeamManagementDbContext _context;

	public TeamRepository(TeamManagementDbContext context)
	{
		_context = context;
	}

	public void AddTeam(Team team) => _context.Teams.Add(team);

	public void RemoveTeam(Team team) => _context.Teams.Remove(team);

	public async Task<Team?> GetTeamByIdAsync(TeamId teamId, CancellationToken ct = default)
	{
		return await _context.Teams
			.AsSplitQuery()
			.Include(team => team.Members)
			.Include(team => team.EventTypes)
			.FirstOrDefaultAsync(team => team.Id == teamId, ct);
	}

	public async Task<List<Team>> GetTeamsByUserIdAsync(UserId userId, CancellationToken ct = default)
	{
		return await _context.Teams
			.AsSplitQuery()
			.Include(team => team.Members)
			.Include(team => team.EventTypes)
			.Where(team => team.Members.Any(member => member.UserId == userId))
			.ToListAsync(ct);
	}
}
