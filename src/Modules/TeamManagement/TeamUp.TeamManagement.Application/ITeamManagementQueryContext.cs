using TeamUp.TeamManagement.Domain.Aggregates.Events;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Users;

namespace TeamUp.TeamManagement.Application;

public interface ITeamManagementQueryContext
{
	public IQueryable<User> Users { get; }
	public IQueryable<Team> Teams { get; }
	public IQueryable<Invitation> Invitations { get; }
	public IQueryable<Event> Events { get; }
}
