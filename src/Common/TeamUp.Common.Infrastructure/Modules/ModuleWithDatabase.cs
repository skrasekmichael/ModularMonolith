using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Application;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.Common.Infrastructure.Services;

namespace TeamUp.Common.Infrastructure.Modules;

public abstract class ModuleWithDatabase<TDatabaseContext> : Module, IModuleWithDatabase where TDatabaseContext : DbContext, IDatabaseContext
{
	public void ConfigureDatabase(IServiceCollection services)
	{
		services.AddDbContext<TDatabaseContext>((serviceProvide, optionsBuilder) =>
		{
			var configurator = serviceProvide.GetRequiredService<IDbContextConfigurator>();
			configurator.Configure<TDatabaseContext>(optionsBuilder);
		});

		services.AddKeyedScoped<IUnitOfWork, UnitOfWork<TDatabaseContext>>(TDatabaseContext.ModuleName);
	}

	(string Name, string Schema) IModuleWithDatabase.GetMigrationTable() => DatabaseUtils.GetMigrationsTable<TDatabaseContext>();

	DbContext IModuleWithDatabase.CreateDatabaseContext(string connectionString) => DatabaseUtils.CreateDatabaseContext<TDatabaseContext>(connectionString);

	public DbContext GetDatabaseContext(AsyncServiceScope scope) => scope.ServiceProvider.GetRequiredService<TDatabaseContext>();
}
