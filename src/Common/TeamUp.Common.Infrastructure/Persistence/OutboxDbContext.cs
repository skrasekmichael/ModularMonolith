using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace TeamUp.Common.Infrastructure.Persistence;

internal sealed class OutboxDbContext(DbContextOptions<OutboxDbContext> options) : DbContext(options), IDatabaseContext
{
	public static string ModuleName => "InfrastructureOutbox";

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(ModuleName);
		modelBuilder.AddTransactionalOutboxEntities();

		base.OnModelCreating(modelBuilder);
	}
}
