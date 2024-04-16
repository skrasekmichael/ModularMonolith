using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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

	public void Configure<TDatabaseContext>(DbContextOptionsBuilder optionsBuilder) where TDatabaseContext : DbContext, IDatabaseContext
	{
		optionsBuilder.UseNpgsql(_options.Value.ConnectionString, options =>
		{
			options.MigrationsAssembly(typeof(TDatabaseContext).Assembly.GetName().Name);
			options.MigrationsHistoryTable(IDatabaseContext.MIGRATIONS_HISTORY_TABLE, TDatabaseContext.ModuleName);
		});
	}
}
