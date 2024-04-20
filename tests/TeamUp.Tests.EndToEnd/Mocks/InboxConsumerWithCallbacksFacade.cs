using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TeamUp.Common.Infrastructure.Processing.Inbox;

internal sealed class InboxConsumerWithCallbacksFacade : IInboxConsumer
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<InboxConsumerWithCallbacksFacade> _logger;
	private readonly InboxConsumer _inboxConsumer;

	public InboxConsumerWithCallbacksFacade(IServiceProvider serviceProvider, ILogger<InboxConsumerWithCallbacksFacade> logger, InboxConsumer inboxConsumer)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
		_inboxConsumer = inboxConsumer;
	}

	public async Task DispatchIntegrationEventsAsync(DbContext dbContext, CancellationToken ct = default)
	{
		_logger.LogInformation("Retrieving inbox messages.");

		//get unpublished integration events
		var messages = await InboxConsumer.GetInboxAsync(dbContext, ct);

		_logger.LogInformation("Publishing inbox messages.");

		//publish integration events
		var completedHandlers = new List<Type>(messages.Count);
		foreach (var message in messages)
		{
			var handlerType = await _inboxConsumer.DispatchEventAsync(message, ct);
			if (handlerType is not null)
			{
				completedHandlers.Add(handlerType);
			}
		}

		await dbContext.SaveChangesAsync(ct);

		//notify about completion of handling integration events
		foreach (var handlerType in completedHandlers)
		{
			var callbackType = typeof(Owner<,>).MakeGenericType(handlerType, typeof(CallbackCounter));
			var callback = Unsafe.As<Owner<object, CallbackCounter>>(_serviceProvider.GetRequiredService(callbackType));
			callback.Service.Invoke();
		}
	}
}
