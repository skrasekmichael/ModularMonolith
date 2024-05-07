using System.Reflection;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Quartz;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.Common.Domain;
using TeamUp.Common.Infrastructure.Extensions;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.Common.Infrastructure.Processing.Commands;
using TeamUp.Common.Infrastructure.Processing.Inbox;
using TeamUp.Common.Infrastructure.Processing.IntegrationEvents;
using TeamUp.Common.Infrastructure.Processing.Outbox;
using TeamUp.Common.Infrastructure.Processing.Queries;
using TeamUp.Common.Infrastructure.Services;

namespace TeamUp.Common.Infrastructure.Modules;

public abstract class Module<TModuleId, TDatabaseContext> : IModule
	where TModuleId : class, IModuleId
	where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
{
	private static readonly Type QueryType = typeof(IQuery<>);
	private static readonly Type QueryConsumerType = typeof(QueryConsumerFacade<,>);
	private static readonly Type QueryConsumerDefinitionType = typeof(QueryConsumerDefinition<,>);
	private static readonly Type CommandType = typeof(ICommand);
	private static readonly Type CommandConsumerType = typeof(CommandConsumerFacade<>);
	private static readonly Type CommandWithResponseType = typeof(ICommand<>);
	private static readonly Type CommandWithResponseConsumerType = typeof(CommandConsumerFacade<,>);
	private static readonly Type IntegrationEventHandlerType = typeof(IIntegrationEventHandler<>);
	private static readonly Type IntegrationEventConsumerType = typeof(IntegrationEventConsumerFacade<,,>);

	private static readonly JobKey ProcessOutboxJobKey = new(TModuleId.ModuleName + "Outbox");
	private static readonly JobKey CleanProcessedOutboxMessagesJobKey = new(TModuleId.ModuleName + "CleanOutbox");
	private static readonly JobKey ProcessInboxJobKey = new(TModuleId.ModuleName + "Inbox");
	private static readonly JobKey CleanProcessedInboxMessagesJobKey = new(TModuleId.ModuleName + "CleanInbox");

	public abstract Assembly ContractsAssembly { get; }
	public abstract Assembly ApplicationAssembly { get; }
	public abstract Assembly[] Assemblies { get; }

	public abstract void ConfigureServices(IServiceCollection services);
	public virtual void ConfigureHealthChecks(IHealthChecksBuilder healthChecks) { }
	public virtual void ConfigureJobs(IServiceCollectionQuartzConfigurator configurator) { }

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
				var consumerDefinition = QueryConsumerDefinitionType.MakeGenericType(type, responseType);

				cfg.AddConsumer(consumerType, consumerDefinition);
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

			if (type.ImplementInterfaceOfType(CommandType))
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
				services.AddScoped(type); //register handler
				var integrationEvent = eventHandlerInterface.GetGenericType()
					?? throw new InternalException($"Unexpected generic type when registering integration event consumer in module '{GetType().Name}'.");

				var consumerType = IntegrationEventConsumerType.MakeGenericType(typeof(TModuleId), type, integrationEvent);
				cfg.AddConsumer(consumerType); //register consumer
			}
		}
	}

	public void ConfigureEssentialServices(IServiceCollection services, IHealthChecksBuilder healthChecks)
	{
		services.AddDbContext<TDatabaseContext>((serviceProvide, optionsBuilder) =>
		{
			var t = serviceProvide.GetType();
			var configurator = serviceProvide.GetService<IDbContextConfigurator>()!;
			configurator.Configure<TDatabaseContext, TModuleId>(optionsBuilder);
		});

		healthChecks.AddDbContextCheck<TDatabaseContext>();

		services
			.AddScoped<IUnitOfWork<TModuleId>, UnitOfWork<TDatabaseContext, TModuleId>>()
			.AddScoped<IIntegrationEventPublisher<TModuleId>, IntegrationEventPublisher<TDatabaseContext, TModuleId>>()
			.AddScoped<IInboxProducer<TModuleId>, InboxProducer<TDatabaseContext, TModuleId>>()
			.AddScoped<IProcessOutboxMessagesJob<TModuleId>, ProcessOutboxMessagesJob<TDatabaseContext, TModuleId>>()
			.AddScoped<ICleanProcessedOutboxMessagesJob<TModuleId>, CleanProcessedOutboxMessagesJob<TDatabaseContext, TModuleId>>()
			.AddScoped<IProcessInboxMessagesJob<TModuleId>, ProcessInboxMessagesJob<TDatabaseContext, TModuleId>>()
			.AddScoped<ICleanProcessedInboxMessagesJob<TModuleId>, CleanProcessedInboxMessagesJob<TDatabaseContext, TModuleId>>();
	}

	(string Name, string Schema) IModule.GetMigrationTable() => DatabaseUtils.GetMigrationsTable<TDatabaseContext, TModuleId>();

	DbContext IModule.CreateDatabaseContext(string connectionString) => DatabaseUtils.CreateDatabaseContext<TDatabaseContext, TModuleId>(connectionString);

	public DbContext GetDatabaseContext<TScope>(TScope scope) where TScope : IServiceScope =>
		scope.ServiceProvider.GetRequiredService<TDatabaseContext>();

	public void ConfigureEssentialJobs(IServiceCollectionQuartzConfigurator configurator)
	{
		configurator
			.AddJob<IProcessOutboxMessagesJob<TModuleId>>(ProcessOutboxJobKey)
			.AddTrigger(trigger =>
			{
				trigger
					.ForJob(ProcessOutboxJobKey)
					.WithSimpleSchedule(schedule => schedule
						.WithIntervalInSeconds(2)
						.RepeatForever());
			})
			.AddJob<IProcessInboxMessagesJob<TModuleId>>(ProcessInboxJobKey)
			.AddTrigger(trigger =>
			{
				trigger
					.ForJob(ProcessInboxJobKey)
					.WithSimpleSchedule(schedule => schedule
						.WithIntervalInSeconds(2)
						.RepeatForever());
			})
			.AddJob<ICleanProcessedOutboxMessagesJob<TModuleId>>(CleanProcessedOutboxMessagesJobKey)
			.AddTrigger(trigger =>
			{
				trigger
					.ForJob(CleanProcessedOutboxMessagesJobKey)
					.WithSimpleSchedule(schedule => schedule
						.WithIntervalInMinutes(5)
						.RepeatForever());
			})
			.AddJob<ICleanProcessedInboxMessagesJob<TModuleId>>(CleanProcessedInboxMessagesJobKey)
			.AddTrigger(trigger =>
			{
				trigger
					.ForJob(CleanProcessedInboxMessagesJobKey)
					.WithSimpleSchedule(schedule => schedule
						.WithIntervalInHours(23)
						.RepeatForever());
			});
	}
}
