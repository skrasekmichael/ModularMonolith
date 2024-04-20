using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Infrastructure.Extensions;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.Notifications.Contracts;

namespace TeamUp.Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options), IDatabaseContext<Contracts.NotificationsModuleId>
{
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(NotificationsModuleId.ModuleName);

		modelBuilder.ConfigureOutbox();
	}
}
