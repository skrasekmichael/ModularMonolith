using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Contracts;
using TeamUp.Common.Domain;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.Common.Infrastructure.Processing.Outbox;

namespace TeamUp.Common.Infrastructure.Services;

internal sealed class IntegrationEventPublisher<TDatabaseContext, TModuleId> : IIntegrationEventPublisher<TModuleId>
	where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
	where TModuleId : IModuleId
{
	private static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		WriteIndented = false
	};

	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly TDatabaseContext _dbContext;

	public IntegrationEventPublisher(IDateTimeProvider dateTimeProvider, TDatabaseContext dbContext)
	{
		_dateTimeProvider = dateTimeProvider;
		_dbContext = dbContext;
	}

	public void Publish<TEvent>(TEvent integrationEvent) where TEvent : class, IIntegrationEvent
	{
		var message = new OutboxMessage
		{
			Id = Guid.NewGuid(),
			CreatedUtc = _dateTimeProvider.UtcNow,
			Assembly = integrationEvent.GetType().Assembly.GetName().Name!,
			Type = integrationEvent.GetType().FullName!,
			Data = JsonSerializer.Serialize(integrationEvent, JsonSerializerOptions)
		};

		_dbContext.Set<OutboxMessage>().Add(message);
	}
}
