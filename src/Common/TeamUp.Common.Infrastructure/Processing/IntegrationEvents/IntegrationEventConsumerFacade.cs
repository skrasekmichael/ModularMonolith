using MassTransit;

using Microsoft.Extensions.Logging;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Processing.Inbox;

namespace TeamUp.Common.Infrastructure.Processing.IntegrationEvents;

internal sealed class IntegrationEventConsumerFacade<TModuleId, TIntegrationEventHandler, TIntegrationEvent> : IConsumer<TIntegrationEvent>
	where TModuleId : IModuleId
	where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
	where TIntegrationEvent : class, IIntegrationEvent
{
	private readonly IInboxProducer<TModuleId> _inboxProducer;
	private readonly ILogger<IntegrationEventConsumerFacade<TModuleId, TIntegrationEventHandler, TIntegrationEvent>> _logger;

	public IntegrationEventConsumerFacade(
		IInboxProducer<TModuleId> inboxProducer,
		ILogger<IntegrationEventConsumerFacade<TModuleId, TIntegrationEventHandler, TIntegrationEvent>> logger)
	{
		_inboxProducer = inboxProducer;
		_logger = logger;
	}

	public Task Consume(ConsumeContext<TIntegrationEvent> context)
	{
		_logger.LogInformation("Consuming Integration Event {eventType} for {eventHandlerType} from bus.", typeof(TIntegrationEvent).Name, typeof(TIntegrationEventHandler).Name);

		return _inboxProducer.ProduceEventAsync<TIntegrationEventHandler, TIntegrationEvent>(context.Message, context.CancellationToken);
	}
}
