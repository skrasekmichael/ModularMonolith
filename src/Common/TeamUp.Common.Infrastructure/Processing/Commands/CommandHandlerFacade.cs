using MassTransit;

using MediatR;

using TeamUp.Application.Abstractions;

namespace TeamUp.Common.Infrastructure.Processing.Commands;

internal sealed class CommandHandlerFacade<TCommand> : IConsumer<TCommand> where TCommand : class, ICommand
{
	private readonly ISender _sender;

	public CommandHandlerFacade(ISender sender)
	{
		_sender = sender;
	}

	public async Task Consume(ConsumeContext<TCommand> context)
	{
		var result = await _sender.Send(context.Message, context.CancellationToken);
		await context.RespondAsync(result);
	}
}

internal sealed class CommandHandlerFacade<TCommand, TResponse> : IConsumer<TCommand> where TCommand : class, ICommand<TResponse>
{
	private readonly ISender _sender;

	public CommandHandlerFacade(ISender sender)
	{
		_sender = sender;
	}

	public async Task Consume(ConsumeContext<TCommand> context)
	{
		var result = await _sender.Send(context.Message, context.CancellationToken);
		await context.RespondAsync(result);
	}
}
