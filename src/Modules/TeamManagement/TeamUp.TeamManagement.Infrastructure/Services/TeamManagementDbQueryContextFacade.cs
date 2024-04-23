using Microsoft.EntityFrameworkCore;

using TeamUp.TeamManagement.Application;
using TeamUp.TeamManagement.Domain.Aggregates.Events;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Users;

namespace TeamUp.TeamManagement.Infrastructure.Services;

internal sealed class TeamManagementDbQueryContextFacade : ITeamManagementQueryContext
{
	private readonly TeamManagementDbContext _context;

	public TeamManagementDbQueryContextFacade(TeamManagementDbContext context)
	{
		_context = context;
	}

	public IQueryable<User> Users => _context.Users.AsNoTracking();
	public IQueryable<Team> Teams => _context.Teams.AsNoTracking();
	public IQueryable<Event> Events => _context.Events.AsNoTracking();
	public IQueryable<Invitation> Invitations => _context.Invitations.AsNoTracking();
}
