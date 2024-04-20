using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Domain;
using TeamUp.Common.Infrastructure.Extensions;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.TeamManagement.Domain.Aggregates;

namespace TeamUp.TeamManagement.Infrastructure;

public sealed class TeamManagementDbContext(DbContextOptions<TeamManagementDbContext> options) : DbContext(options), IDatabaseContext<Contracts.TeamManagementModuleId>
{
	public DbSet<User> Users => Set<User>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(Contracts.TeamManagementModuleId.ModuleName);

		modelBuilder.Ignore<List<IDomainEvent>>();
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(TeamManagementDbContext).Assembly);

		modelBuilder.ConfigureOutbox();
	}
}
