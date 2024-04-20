using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Modules;
using TeamUp.Common.Infrastructure.Options;
using TeamUp.Common.Infrastructure.Processing;
using TeamUp.Common.Infrastructure.Processing.Inbox;
using TeamUp.Common.Infrastructure.Processing.Outbox;
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

	public static IServiceCollection AddAppOptions<TOptions>(this IServiceCollection services, Action<TOptions> configure) where TOptions : class, IAppOptions
	{
		services.AddOptions<TOptions>()
			.BindConfiguration(TOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart()
			.PostConfigure(configure);

		return services;
	}

	public static IServiceCollection AddInfrastructure(this IServiceCollection services)
	{
		services
			.AddAppOptions<DatabaseOptions>()
			.AddAppOptions<RabbitMqOptions>()
			.AddAppOptions<ClientOptions>()
			.AddAppOptions<JwtOptions>();

		services
			.AddSingleton<IDateTimeProvider, DateTimeProvider>()
			.AddSingleton<IDbContextConfigurator, DbContextConfigurator>();

		services.AddScoped<DomainEventsDispatcher>();
		services.AddScoped<OutboxConsumer>();
		services.AddScoped<IInboxConsumer, InboxConsumer>();

		return services;
	}

	public static IEnumerable<IModule> AddModules(this IServiceCollection services, Action<ModulesBuilder> configure)
	{
		var modules = new ModulesBuilder();
		configure(modules);

		var healthChecks = services.AddHealthChecks();

		foreach (var module in modules)
		{
			services.AddValidatorsFromAssembly(module.ContractsAssembly);

			module.ConfigureServices(services);
			module.ConfigureHealthChecks(healthChecks);
			module.ConfigureEssentialServices(services, healthChecks);
		}

		modules.ConfigureMassTransit(services);
		modules.ConfigureMediatR(services);
		modules.ConfigureBackgroundJobs(services);

		return modules;
	}
}
