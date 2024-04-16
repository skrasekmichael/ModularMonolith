using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Domain;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Infrastructure.Persistence;

public sealed class UserAccessDbContext(DbContextOptions<UserAccessDbContext> options) : DbContext(options), IDatabaseContext
{
	public static string ModuleName => Constants.MODULE_NAME;

	public DbSet<User> Users => Set<User>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(ModuleName);

		modelBuilder.Ignore<List<IDomainEvent>>();
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserAccessDbContext).Assembly);

		base.OnModelCreating(modelBuilder);
	}
}
