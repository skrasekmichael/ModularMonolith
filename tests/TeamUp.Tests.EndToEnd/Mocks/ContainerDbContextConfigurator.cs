using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.Common.Infrastructure.Services;

namespace TeamUp.Tests.EndToEnd.Mocks;

internal sealed class ContainerDbContextConfigurator(string connectionString) : IDbContextConfigurator
{
	public void Configure<TDatabaseContext>(DbContextOptionsBuilder optionsBuilder) where TDatabaseContext : DbContext, IDatabaseContext
	{
		optionsBuilder.UseNpgsql(connectionString, options =>
		{
			options.MigrationsAssembly(typeof(TDatabaseContext).Assembly.GetName().Name);
			options.MigrationsHistoryTable(IDatabaseContext.MIGRATIONS_HISTORY_TABLE, TDatabaseContext.ModuleName);
		});

		optionsBuilder.ConfigureWarnings(warning =>
		{
			warning.Log(RelationalEventId.MultipleCollectionIncludeWarning);
		});
	}
}
