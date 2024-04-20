using TeamUp.Common.Infrastructure.Persistence;

namespace TeamUp.Notifications.Infrastructure.Persistence;

internal sealed class DesignTimeDbContextFactory : DesignTimeDatabaseContextFactory<NotificationsDbContext, Contracts.NotificationsModuleId>;
