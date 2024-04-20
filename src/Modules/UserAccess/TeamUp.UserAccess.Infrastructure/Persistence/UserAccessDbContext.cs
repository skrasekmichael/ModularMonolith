using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Domain;
using TeamUp.Common.Infrastructure.Extensions;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Infrastructure.Persistence;

public sealed class UserAccessDbContext(DbContextOptions<UserAccessDbContext> options) : DbContext(options), IDatabaseContext<Contracts.UserAccessModuleId>
{
	public DbSet<User> Users => Set<User>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(Contracts.UserAccessModuleId.ModuleName);

		modelBuilder.Ignore<List<IDomainEvent>>();
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserAccessDbContext).Assembly);

		modelBuilder.ConfigureOutbox();
	}
}
