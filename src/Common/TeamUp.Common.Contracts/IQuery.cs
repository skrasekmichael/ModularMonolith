using MediatR;

using RailwayResult;

namespace TeamUp.Application.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>, IQueryPipeline;

public interface IQueryPipeline;
