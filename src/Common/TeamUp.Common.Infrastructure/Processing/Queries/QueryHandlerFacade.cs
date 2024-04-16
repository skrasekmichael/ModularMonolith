using MassTransit;

using MediatR;

using TeamUp.Application.Abstractions;

namespace TeamUp.Common.Infrastructure.Processing.Queries;

internal sealed class QueryHandlerFacade<TQuery, TResponse> : IConsumer<TQuery> where TQuery : class, IQuery<TResponse>
{
	private readonly ISender _sender;

	public QueryHandlerFacade(ISender sender)
	{
		_sender = sender;
	}

	public async Task Consume(ConsumeContext<TQuery> context)
	{
		var result = await _sender.Send(context.Message, context.CancellationToken);
		await context.RespondAsync(result);
	}
}
