using System.Collections;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using TeamUp.Common.Infrastructure.Options;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.Common.Infrastructure.Processing.Commands;
using TeamUp.Common.Infrastructure.Processing.Queries;

namespace TeamUp.Common.Infrastructure.Modules;

public sealed class ModulesConfigurator : IEnumerable<IModule>
{
	private readonly List<IModule> _modules = [];

	public ModulesConfigurator AddModule<TModule>() where TModule : IModule, new()
	{
		var module = new TModule();
		_modules.Add(module);
		return this;
	}

	public IEnumerator<IModule> GetEnumerator() => _modules.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _modules.GetEnumerator();

	internal void ConfigureMediatR(IServiceCollection services)
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

	internal void ConfigureMassTransit(IServiceCollection services)
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

				cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
				cfg.UseMessageRetry(r => r.Immediate(5));

				cfg.ConfigureEndpoints(context);
			});

			cfg.AddEntityFrameworkOutbox<OutboxDbContext>(options =>
			{
				options.UsePostgres();
				options.UseBusOutbox();
			});

			cfg.AddConfigureEndpointsCallback((context, name, cfg) =>
			{
				cfg.UseEntityFrameworkOutbox<OutboxDbContext>(context);
			});
		});
	}
}
