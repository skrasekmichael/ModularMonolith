using MediatR;

using RailwayResult;

namespace TeamUp.Common.Contracts;

public interface ICommand : IRequest<Result>, ICommandPipeline;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, ICommandPipeline;

public interface ICommandPipeline;
