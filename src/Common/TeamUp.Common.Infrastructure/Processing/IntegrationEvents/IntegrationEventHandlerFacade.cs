using System.Data;
using System.Transactions;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Npgsql;

using RailwayResult;
using RailwayResult.Errors;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts.Errors;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.Common.Infrastructure.Services;
using TeamUp.Domain.Abstractions;

namespace TeamUp.Common.Infrastructure.Processing.IntegrationEvents;

internal sealed class IntegrationEventHandlerFacade<THandler, TIntegrationEvent> : IConsumer<TIntegrationEvent>
	where THandler : IIntegrationEventHandler<TIntegrationEvent>
	where TIntegrationEvent : class, IIntegrationEvent
{
	private static readonly InternalError UnexpectedError = new("IntegrationEventConsumer.InternalError", "Error occurred while consuming integration event.");

	private readonly OutboxDbContext _outboxDbContext;
	private readonly THandler _handler;
	private readonly ILogger<IntegrationEventHandlerFacade<THandler, TIntegrationEvent>> _logger;

	public IntegrationEventHandlerFacade(OutboxDbContext outboxDbContext, THandler handler, ILogger<IntegrationEventHandlerFacade<THandler, TIntegrationEvent>> logger)
	{
		_outboxDbContext = outboxDbContext;
		_handler = handler;
		_logger = logger;
	}

	public async Task Consume(ConsumeContext<TIntegrationEvent> context)
	{
		_logger.LogInformation("Consuming Integration Event {event} ...", typeof(TIntegrationEvent).Name);

		var result = await HandleAsync(context.Message, context.CancellationToken);
		if (result.IsFailure)
		{
			if (result is { Error: ConcurrencyError or InternalError })
			{
				//this errors may be resolved by retrying to consume the event
				_logger.LogCritical("Failed to consume integration event {eventData}.", context.Message);
				throw new RetryToConsumeIntegrationEventException(result.Error);
			}

			//probably bug, discard event
			_logger.LogWarning("Failed to consume integration event {eventData}, error {error} occurred.", context.Message, result.Error);
		}
	}

	private async Task<Result> HandleAsync(TIntegrationEvent message, CancellationToken ct)
	{
		using var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
		try
		{
			var result = await _handler.Handle(message, ct);
			if (result.IsSuccess)
			{
				await _outboxDbContext.SaveChangesAsync(ct);
				scope.Complete();
			}

			return result;
		}
		catch (DBConcurrencyException)
		{
			return UnitOfWork<OutboxDbContext>.ConcurrencyError;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.InnerException, "Database Outbox Update Exception");

			if (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
			{
				return UnitOfWork<OutboxDbContext>.UniqueConstraintError;
			}

			return UnitOfWork<OutboxDbContext>.UnexpectedError;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while consuming integration event {event}.", typeof(TIntegrationEvent).Name);
			return UnexpectedError;
		}
	}
}
