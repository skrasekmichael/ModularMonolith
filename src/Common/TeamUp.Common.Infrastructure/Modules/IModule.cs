using System.Reflection;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Quartz;

namespace TeamUp.Common.Infrastructure.Modules;

public interface IModule
{
	public Assembly ContractsAssembly { get; }
	public Assembly ApplicationAssembly { get; }
	public Assembly[] Assemblies { get; }

	public void ConfigureServices(IServiceCollection services);
	public void ConfigureHealthChecks(IHealthChecksBuilder healthChecks);
	public void RegisterRequestConsumers(IBusRegistrationConfigurator cfg);
	public void RegisterEventConsumers(IServiceCollection services, IBusRegistrationConfigurator cfg);
	public void ConfigureEssentialServices(IServiceCollection services, IHealthChecksBuilder healthChecks);
	public void ConfigureJobs(IServiceCollectionQuartzConfigurator configurator);

	internal abstract DbContext CreateDatabaseContext(string connectionString);
	internal (string Name, string Schema) GetMigrationTable();

	public DbContext GetDatabaseContext<TScope>(TScope scope) where TScope : IServiceScope;
}
