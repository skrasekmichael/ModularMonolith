using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TeamUp.Common.Infrastructure.Modules;

public interface IModuleWithDatabase : IModule
{
	public void ConfigureDatabase(IServiceCollection services);

	internal abstract DbContext CreateDatabaseContext(string connectionString);
	internal (string Name, string Schema) GetMigrationTable();

	public DbContext GetDatabaseContext(AsyncServiceScope scope);
}
