using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Extensions;
using TeamUp.Common.Infrastructure.Modules;
using TeamUp.Common.Infrastructure.Options;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.Common.Infrastructure.Processing;
using TeamUp.Common.Infrastructure.Services;

namespace TeamUp.Common.Infrastructure;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddAppOptions<TOptions>(this IServiceCollection services) where TOptions : class, IAppOptions
	{
		services.AddOptions<TOptions>()
			.BindConfiguration(TOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		return services;
	}

	public static IEnumerable<IModule> AddInfrastructure(this IServiceCollection services, Action<ModulesConfigurator> configure)
	{
		services
			.AddAppOptions<DatabaseOptions>()
			.AddAppOptions<RabbitMqOptions>()
			.AddAppOptions<ClientOptions>()
			.AddAppOptions<JwtOptions>();

		services
			.AddSingleton<IDateTimeProvider, DateTimeProvider>()
			.AddSingleton<IDbContextConfigurator, DbContextConfigurator>()
			.AddScoped<DomainEventsDispatcher>();

		services.AddDbContext<OutboxDbContext>((serviceProvide, optionsBuilder) =>
		{
			var configurator = serviceProvide.GetRequiredService<IDbContextConfigurator>();
			configurator.Configure<OutboxDbContext>(optionsBuilder);
		});

		var modules = new ModulesConfigurator();
		configure(modules);

		var healthChecks = services.AddHealthChecks();

		foreach (var module in modules)
		{
			module.ConfigureServices(services);
			module.ConfigureHealthChecks(healthChecks);
			services.AddValidatorsFromAssembly(module.ContractsAssembly);

			if (module is IModuleWithDatabase dbModule)
			{
				dbModule.ConfigureDatabase(services);
			}
		}

		modules.ConfigureMassTransit(services);
		modules.ConfigureMediatR(services);

		return modules;
	}
}
