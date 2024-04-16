using Microsoft.EntityFrameworkCore;

namespace TeamUp.Common.Infrastructure.Persistence;

public static class DatabaseUtils
{
	public static TDatabaseContext CreateDatabaseContext<TDatabaseContext>(string connectionString) where TDatabaseContext : DbContext, IDatabaseContext
	{
		var optionsBuilder = new DbContextOptionsBuilder<TDatabaseContext>();
		optionsBuilder.UseNpgsql(connectionString, options =>
		{
			options.MigrationsAssembly(typeof(TDatabaseContext).Assembly.GetName().Name);
			options.MigrationsHistoryTable(IDatabaseContext.MIGRATIONS_HISTORY_TABLE, TDatabaseContext.ModuleName);
		});

		return (TDatabaseContext)Activator.CreateInstance(typeof(TDatabaseContext), optionsBuilder.Options)!;
	}

	public static (string Name, string Schema) GetMigrationsTable<TDatabaseContext>() where TDatabaseContext : DbContext, IDatabaseContext => (IDatabaseContext.MIGRATIONS_HISTORY_TABLE, TDatabaseContext.ModuleName);
}
