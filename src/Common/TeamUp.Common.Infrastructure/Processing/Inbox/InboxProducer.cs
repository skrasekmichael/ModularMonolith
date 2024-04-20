using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Persistence;

namespace TeamUp.Common.Infrastructure.Processing.Inbox;

internal interface IInboxProducer<TModuleId> where TModuleId : IModuleId
{
	public Task ProduceEventAsync<TIntegrationEventHandler, TEvent>(TEvent integrationEvent, CancellationToken ct)
		where TIntegrationEventHandler : IIntegrationEventHandler<TEvent>
		where TEvent : class, IIntegrationEvent;
}

internal sealed class InboxProducer<TDatabaseContext, TModuleId> : IInboxProducer<TModuleId>
	where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
	where TModuleId : IModuleId
{
	private static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		WriteIndented = false
	};

	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly TDatabaseContext _dbContext;

	public InboxProducer(IDateTimeProvider dateTimeProvider, TDatabaseContext dbContext)
	{
		_dateTimeProvider = dateTimeProvider;
		_dbContext = dbContext;
	}

	public Task ProduceEventAsync<TIntegrationEventHandler, TEvent>(TEvent integrationEvent, CancellationToken ct)
		where TIntegrationEventHandler : IIntegrationEventHandler<TEvent>
		where TEvent : class, IIntegrationEvent
	{
		var type = typeof(TIntegrationEventHandler);

		var message = new InboxMessage
		{
			Id = Guid.NewGuid(),
			CreatedUtc = _dateTimeProvider.UtcNow,
			Assembly = type.Assembly.GetName().Name!,
			Type = type.FullName!,
			Data = JsonSerializer.Serialize(integrationEvent, JsonSerializerOptions)
		};

		_dbContext.Set<InboxMessage>().Add(message);
		return _dbContext.SaveChangesAsync(ct);
	}
}
