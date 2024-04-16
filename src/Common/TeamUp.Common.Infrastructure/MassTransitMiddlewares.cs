using System.Transactions;

using MassTransit;
using MassTransit.Configuration;

using TeamUp.Domain.Abstractions;

namespace TeamUp.Common.Infrastructure;

public sealed class IntegrationEventTransactionFilter<TEvent> : IFilter<ConsumeContext<TEvent>> where TEvent : class, PipeContext, IIntegrationEvent
{
	public void Probe(ProbeContext context)
	{
		context.CreateFilterScope("transaction");
	}

	public async Task Send(ConsumeContext<TEvent> context, IPipe<ConsumeContext<TEvent>> next)
	{
		// do something
		using var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
		try
		{

			await next.Send(context);

			//await _outboxDbContext.SaveChangesAsync(ct);

			scope.Complete();
		}
		catch
		{
			throw;
		}
	}
}
