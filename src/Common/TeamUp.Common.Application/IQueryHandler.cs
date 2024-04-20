using MediatR;

using RailwayResult;

using TeamUp.Common.Contracts;

namespace TeamUp.Common.Application;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : IQuery<TResponse>
{
	public new Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct);
};
