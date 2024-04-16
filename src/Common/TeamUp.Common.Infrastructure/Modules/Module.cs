using System.Reflection;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;

using TeamUp.Application.Abstractions;
using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Extensions;
using TeamUp.Common.Infrastructure.Processing.Commands;
using TeamUp.Common.Infrastructure.Processing.IntegrationEvents;
using TeamUp.Common.Infrastructure.Processing.Queries;

namespace TeamUp.Common.Infrastructure.Modules;

public abstract class Module : IModule
{
	private static readonly Type QueryType = typeof(IQuery<>);
	private static readonly Type QueryConsumerType = typeof(QueryHandlerFacade<,>);
	private static readonly Type CommandType = typeof(ICommand);
	private static readonly Type CommandConsumerType = typeof(CommandHandlerFacade<>);
	private static readonly Type CommandWithResponseType = typeof(ICommand<>);
	private static readonly Type CommandWithResponseConsumerType = typeof(CommandHandlerFacade<,>);
	private static readonly Type IntegrationEventHandlerType = typeof(IIntegrationEventHandler<>);
	private static readonly Type IntegrationEventConsumerType = typeof(IntegrationEventHandlerFacade<,>);

	public abstract Assembly ContractsAssembly { get; }
	public abstract Assembly ApplicationAssembly { get; }
	public abstract Assembly[] Assemblies { get; }

	public abstract void ConfigureServices(IServiceCollection services);
	public virtual void ConfigureHealthChecks(IHealthChecksBuilder healthChecks) { }

	public void RegisterRequestConsumers(IBusRegistrationConfigurator cfg)
	{
		var types = ContractsAssembly
			.GetTypes()
			.Where(type => type.IsClass && !type.IsAbstract);

		foreach (var type in types)
		{
			var queryInterface = type.GetInterfaceWithGenericDefinition(QueryType);
			if (queryInterface is not null)
			{
				var responseType = queryInterface.GetGenericType()
					?? throw new InternalException($"Unexpected generic type when registering query consumer in module '{GetType().Name}'.");

				var consumerType = QueryConsumerType.MakeGenericType(type, responseType);
				cfg.AddConsumer(consumerType);
				continue;
			}

			var commandWithResponseInterface = type.GetInterfaceWithGenericDefinition(CommandWithResponseType);
			if (commandWithResponseInterface is not null)
			{
				var responseType = commandWithResponseInterface.GetGenericType()
					?? throw new InternalException($"Unexpected generic type when registering command consumer in module '{GetType().Name}'.");

				var consumerType = CommandWithResponseConsumerType.MakeGenericType(type, responseType);
				cfg.AddConsumer(consumerType);
				continue;
			}

			var commandInterface = type.GetInterfaceWithGenericDefinition(CommandType);
			if (commandInterface is not null)
			{
				var consumerType = CommandConsumerType.MakeGenericType(type);
				cfg.AddConsumer(consumerType);
				continue;
			}
		}
	}

	public void RegisterEventConsumers(IServiceCollection services, IBusRegistrationConfigurator cfg)
	{
		var types = ApplicationAssembly
			.GetTypes()
			.Where(type => type.IsClass && !type.IsAbstract);

		foreach (var type in types)
		{
			var eventHandlerInterface = type.GetInterfaceWithGenericDefinition(IntegrationEventHandlerType);
			if (eventHandlerInterface is not null)
			{
				services.AddScoped(type);
				var responseType = eventHandlerInterface.GetGenericType()
					?? throw new InternalException($"Unexpected generic type when registering integration event consumer in module '{GetType().Name}'.");

				var consumerType = IntegrationEventConsumerType.MakeGenericType(type, responseType);
				cfg.AddConsumer(consumerType);
			}
		}
	}
}
