using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using TeamUp.Common.Contracts;

namespace TeamUp.Common.Infrastructure.Persistence;

public abstract class DesignTimeDatabaseContextFactory<TDatabaseContext, TModuleId> : IDesignTimeDbContextFactory<TDatabaseContext>
	where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
	where TModuleId : IModuleId
{
	public TDatabaseContext CreateDbContext(string[] args) => DatabaseUtils.CreateDatabaseContext<TDatabaseContext, TModuleId>("");
}
