using System.Collections;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Quartz;

using TeamUp.Common.Infrastructure.Options;
using TeamUp.Common.Infrastructure.Processing.Commands;
using TeamUp.Common.Infrastructure.Processing.Queries;

namespace TeamUp.Common.Infrastructure.Modules;

public sealed class ModulesBuilder : IEnumerable<IModule>
{
	private readonly List<IModule> _modules = [];

	public ModulesBuilder AddModule<TModule>() where TModule : IModule, new()
	{
		var module = new TModule();
		_modules.Add(module);
		return this;
	}

	public IEnumerator<IModule> GetEnumerator() => _modules.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _modules.GetEnumerator();

	public void ConfigureMediatR(IServiceCollection services)
	{
		services.AddMediatR(config =>
		{
			config.AddOpenBehavior(typeof(CommandBehavior<,>));
			config.AddOpenBehavior(typeof(QueryBehavior<,>));

			foreach (var module in _modules)
			{
				config.RegisterServicesFromAssemblies(module.Assemblies);
			}
		});
	}

	public void ConfigureMassTransit(IServiceCollection services)
	{
		services.AddMassTransit(cfg =>
		{
			cfg.SetKebabCaseEndpointNameFormatter();

			foreach (var module in _modules)
			{
				module.RegisterRequestConsumers(cfg); //command and query handlers
				module.RegisterEventConsumers(services, cfg); //integration event handlers
			}

			cfg.UsingRabbitMq((context, cfg) =>
			{
				cfg.AutoStart = true;

				var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
				cfg.Host(new Uri(options.ConnectionString));

				cfg.UseMessageRetry(r => r.Immediate(3));
				cfg.ConfigureEndpoints(context);
			});

			//cfg.AddInMemoryInboxOutbox();
			cfg.AddConfigureEndpointsCallback((context, name, cfg) =>
			{
				//cfg.UseInMemoryInboxOutbox(context);
			});
		});
	}

	public void ConfigureBackgroundJobs(IServiceCollection services)
	{
		services.AddQuartzHostedService(options =>
		{
			options.WaitForJobsToComplete = true;
			options.AwaitApplicationStarted = true;
			options.StartDelay = TimeSpan.FromSeconds(2);
		});

		services.AddQuartz(configurator =>
		{
			foreach (var module in _modules)
			{
				module.ConfigureJobs(configurator);
			}
		});
	}
}
