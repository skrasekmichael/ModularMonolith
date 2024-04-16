using System.Reflection;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;

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
}
