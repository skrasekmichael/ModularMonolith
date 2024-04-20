using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Options;
using TeamUp.Common.Infrastructure.Persistence;

namespace TeamUp.Common.Infrastructure.Services;

internal sealed class DbContextConfigurator : IDbContextConfigurator
{
	private readonly IOptions<DatabaseOptions> _options;

	public DbContextConfigurator(IOptions<DatabaseOptions> options)
	{
		_options = options;
	}

	public void Configure<TDatabaseContext, TModuleId>(DbContextOptionsBuilder optionsBuilder)
		where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
		where TModuleId : IModuleId
	{
		optionsBuilder.UseNpgsql(_options.Value.ConnectionString, options =>
		{
			var (tableName, schema) = DatabaseUtils.GetMigrationsTable<TDatabaseContext, TModuleId>();
			options.MigrationsAssembly(typeof(TDatabaseContext).Assembly.GetName().Name);
			options.MigrationsHistoryTable(tableName, schema);
		});
	}
}
