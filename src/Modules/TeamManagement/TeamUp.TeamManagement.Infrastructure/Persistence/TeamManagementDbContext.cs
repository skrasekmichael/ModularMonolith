using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Domain;
using TeamUp.Common.Infrastructure.Extensions;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.TeamManagement.Domain.Aggregates.Events;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Users;

namespace TeamUp.TeamManagement.Infrastructure;

public sealed class TeamManagementDbContext(DbContextOptions<TeamManagementDbContext> options) : DbContext(options), IDatabaseContext<Contracts.TeamManagementModuleId>
{
	public DbSet<User> Users => Set<User>();
	public DbSet<Team> Teams => Set<Team>();
	public DbSet<Invitation> Invitations => Set<Invitation>();
	public DbSet<Event> Events => Set<Event>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(Contracts.TeamManagementModuleId.ModuleName);

		modelBuilder.Ignore<List<IDomainEvent>>();
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(TeamManagementDbContext).Assembly);

		modelBuilder.ConfigureOutbox();
	}
}
