using MediatR;

using RailwayResult;

namespace TeamUp.Application.Abstractions;

public interface ICommand : IRequest<Result>, ICommandPipeline;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, ICommandPipeline;

public interface ICommandPipeline;
