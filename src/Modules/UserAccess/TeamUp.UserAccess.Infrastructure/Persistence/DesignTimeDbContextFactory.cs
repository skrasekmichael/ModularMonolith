using TeamUp.Common.Infrastructure.Persistence;

namespace TeamUp.UserAccess.Infrastructure.Persistence;

internal sealed class DesignTimeDbContextFactory : DesignTimeDatabaseContextFactory<UserAccessDbContext, Contracts.UserAccessModuleId>;
