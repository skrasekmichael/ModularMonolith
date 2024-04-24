using MassTransit;

using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Extensions;

namespace TeamUp.Common.Infrastructure.Processing.Queries;

internal sealed class QueryConsumerDefinition<TQuery, TResponse> : ConsumerDefinition<QueryConsumerFacade<TQuery, TResponse>> where TQuery : class, IQuery<TResponse>
{
	public QueryConsumerDefinition()
	{
		EndpointName = typeof(TQuery).ToKebabCase()!;
	}
}
