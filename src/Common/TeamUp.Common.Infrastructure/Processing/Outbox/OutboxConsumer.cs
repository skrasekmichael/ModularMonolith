using System.Reflection;
using System.Text.Json;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using TeamUp.Common.Contracts;

namespace TeamUp.Common.Infrastructure.Processing.Outbox;

internal sealed class OutboxConsumer
{
	private readonly IPublishEndpoint _publisher;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly ILogger<OutboxConsumer> _logger;

	public OutboxConsumer(IPublishEndpoint publisher, IDateTimeProvider dateTimeProvider, ILogger<OutboxConsumer> logger)
	{
		_publisher = publisher;
		_dateTimeProvider = dateTimeProvider;
		_logger = logger;
	}

	private static Type? ResolveType(OutboxMessage message)
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

	private async Task DispatchEventAsync(OutboxMessage message, CancellationToken ct = default)
	{
		var integrationEventType = ResolveType(message);
		if (integrationEventType is null)
		{
			_logger.LogCritical("Failed to identify outbox message type {message}.", message);
			message.Error = "Type not found.";
			return;
		}

		var integrationEvent = JsonSerializer.Deserialize(message.Data, integrationEventType);
		if (integrationEvent is null)
		{
			_logger.LogCritical("Failed to deserialize outbox message {message}.", message);
			message.Error = "Failed to deserialize.";
			return;
		}

		try
		{
			await _publisher.Publish(integrationEvent, ct);
			message.ProcessedUtc = _dateTimeProvider.UtcNow;
			_logger.LogInformation("Event {integrationEventType} published to bus.", message.Type);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to publish event from outbox message {message}.", message);
			message.Error = "Failed to publish event.";
		}
	}

	public async Task DispatchIntegrationEventsAsync(DbContext dbContext, CancellationToken ct = default)
	{
		_logger.LogInformation("Retrieving outbox messages.");

		//get unpublished integration events
		var messages = await dbContext
			.Set<OutboxMessage>()
			.Where(msg => msg.ProcessedUtc == null)
			.OrderBy(msg => msg.CreatedUtc)
			.Take(20)
			.ToListAsync(ct);

		_logger.LogInformation("Publishing outbox messages.");

		//publish integration events
		foreach (var message in messages)
		{
			await DispatchEventAsync(message, ct);
		}
	}
}
