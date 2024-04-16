using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace TeamUp.Common.Infrastructure.Persistence;

public abstract class DesignTimeDatabaseContextFactory<TDatabaseContext> : IDesignTimeDbContextFactory<TDatabaseContext> where TDatabaseContext : DbContext, IDatabaseContext
{
	public TDatabaseContext CreateDbContext(string[] args) => DatabaseUtils.CreateDatabaseContext<TDatabaseContext>("");
}
