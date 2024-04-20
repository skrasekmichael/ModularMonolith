using MassTransit;

using MediatR;

using TeamUp.Common.Contracts;

namespace TeamUp.Common.Infrastructure.Processing.Commands;

internal sealed class CommandConsumerFacade<TCommand> : IConsumer<TCommand> where TCommand : class, ICommand
{
	private readonly ISender _sender;

	public CommandConsumerFacade(ISender sender)
	{
		_sender = sender;
	}

	public async Task Consume(ConsumeContext<TCommand> context)
	{
		var result = await _sender.Send(context.Message, context.CancellationToken);
		await context.RespondAsync(result);
	}
}

internal sealed class CommandConsumerFacade<TCommand, TResponse> : IConsumer<TCommand> where TCommand : class, ICommand<TResponse>
{
	private readonly ISender _sender;

	public CommandConsumerFacade(ISender sender)
	{
		_sender = sender;
	}

	public async Task Consume(ConsumeContext<TCommand> context)
	{
		var result = await _sender.Send(context.Message, context.CancellationToken);
		await context.RespondAsync(result);
	}
}
