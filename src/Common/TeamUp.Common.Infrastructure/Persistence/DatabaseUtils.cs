using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Contracts;

namespace TeamUp.Common.Infrastructure.Persistence;

public static class DatabaseUtils
{
	public static TDatabaseContext CreateDatabaseContext<TDatabaseContext, TModuleId>(string connectionString)
		where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
		where TModuleId : IModuleId
	{
		var optionsBuilder = new DbContextOptionsBuilder<TDatabaseContext>();
		optionsBuilder.UseNpgsql(connectionString, options =>
		{
			options.MigrationsAssembly(typeof(TDatabaseContext).Assembly.GetName().Name);
			options.MigrationsHistoryTable(IDatabaseContext.MIGRATIONS_HISTORY_TABLE, TModuleId.ModuleName);
		});

		return (TDatabaseContext)Activator.CreateInstance(typeof(TDatabaseContext), optionsBuilder.Options)!;
	}

	public static (string Name, string Schema) GetMigrationsTable<TDatabaseContext, TModuleId>()
		where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
		where TModuleId : IModuleId => (IDatabaseContext.MIGRATIONS_HISTORY_TABLE, TModuleId.ModuleName);
}
