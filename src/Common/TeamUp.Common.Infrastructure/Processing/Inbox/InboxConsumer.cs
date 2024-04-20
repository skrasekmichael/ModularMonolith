using System.Reflection;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using RailwayResult;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Extensions;

namespace TeamUp.Common.Infrastructure.Processing.Inbox;

internal interface IInboxConsumer
{
	public Task DispatchIntegrationEventsAsync(DbContext dbContext, CancellationToken ct = default);
}

internal sealed class InboxConsumer : IInboxConsumer
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly ILogger<InboxConsumer> _logger;

	public InboxConsumer(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider, ILogger<InboxConsumer> logger)
	{
		_serviceProvider = serviceProvider;
		_dateTimeProvider = dateTimeProvider;
		_logger = logger;
	}

	private static Type? ResolveType(InboxMessage message)
	{
		try
		{
			var assembly = Assembly.Load(message.Assembly);
			return assembly.GetType(message.Type);
		}
		catch
		{
			return null;
		}
	}

	internal async Task<Type?> DispatchEventAsync(InboxMessage message, CancellationToken ct = default)
	{
		var integrationEventHandlerType = ResolveType(message);
		if (integrationEventHandlerType is null)
		{
			_logger.LogCritical("Failed to identify inbox message type {message}.", message);
			message.Error = "Type not found.";
			return null;
		}

		var integrationEventType = integrationEventHandlerType
			.GetInterfaceWithGenericDefinition(typeof(IIntegrationEventHandler<>))
			.GetGenericType();
		if (integrationEventType is null)
		{
			_logger.LogCritical("Failed to identify integration event type {message}.", message);
			message.Error = "Event not found.";
			return null;
		}

		if (JsonSerializer.Deserialize(message.Data, integrationEventType) is not IIntegrationEvent integrationEvent)
		{
			_logger.LogCritical("Failed to deserialize inbox message {message}.", message);
			message.Error = "Failed to deserialize.";
			return null;
		}

		try
		{
			using var scope = _serviceProvider.CreateScope();
			var handler = scope.ServiceProvider.GetRequiredService(integrationEventHandlerType);

			var methodInfo = integrationEventHandlerType
				.GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.Handle))!;

			var result = await (Task<Result>)methodInfo.Invoke(handler, [integrationEvent, ct])!;
			if (result.IsSuccess)
			{
				message.ProcessedUtc = _dateTimeProvider.UtcNow;
				message.Error = null;
				_logger.LogInformation("Successfully handled integration event {integrationEventType}.", integrationEventType);
				return integrationEventHandlerType;
			}
			else
			{
				_logger.LogWarning("Integration event handling of {integrationEvent} failed. {error}", integrationEvent, result.Error.ToString());
				message.Error = "Event handling failed.";
				return null;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to handle event from inbox message {message}.", message);
			message.Error = "Failed to handle event.";
			return null;
		}
	}

	internal static Task<List<InboxMessage>> GetInboxAsync(DbContext dbContext, CancellationToken ct)
	{
		return dbContext
			.Set<InboxMessage>()
			.Where(msg => msg.ProcessedUtc == null)
			.OrderBy(msg => msg.CreatedUtc)
			.Take(20)
			.ToListAsync(ct);
	}

	public async Task DispatchIntegrationEventsAsync(DbContext dbContext, CancellationToken ct = default)
	{
		_logger.LogInformation("Retrieving inbox messages.");

		//get unpublished integration events
		var messages = await GetInboxAsync(dbContext, ct);

		_logger.LogInformation("Publishing inbox messages.");

		//publish integration events
		foreach (var message in messages)
		{
			await DispatchEventAsync(message, ct);
		}

		await dbContext.SaveChangesAsync(ct);
	}
}
