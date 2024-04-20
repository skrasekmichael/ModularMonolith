using MassTransit;

using MediatR;

using TeamUp.Common.Contracts;

namespace TeamUp.Common.Infrastructure.Processing.Queries;

internal sealed class QueryConsumerFacade<TQuery, TResponse> : IConsumer<TQuery> where TQuery : class, IQuery<TResponse>
{
	private readonly ISender _sender;

	public QueryConsumerFacade(ISender sender)
	{
		_sender = sender;
	}

	public async Task Consume(ConsumeContext<TQuery> context)
	{
		var result = await _sender.Send(context.Message, context.CancellationToken);
		await context.RespondAsync(result);
	}
}
