using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Domain;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Domain.Aggregates;

namespace TeamUp.TeamManagement.Infrastructure;

public sealed class TeamManagementDbContext(DbContextOptions<TeamManagementDbContext> options) : DbContext(options), IDatabaseContext
{
	public static string ModuleName => Constants.MODULE_NAME;

	public DbSet<User> Users => Set<User>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(ModuleName);

		modelBuilder.Ignore<List<IDomainEvent>>();
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(TeamManagementDbContext).Assembly);

		base.OnModelCreating(modelBuilder);
	}
}
