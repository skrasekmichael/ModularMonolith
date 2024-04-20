using MediatR;

using RailwayResult;

namespace TeamUp.Common.Contracts;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>, IQueryPipeline;

public interface IQueryPipeline;
