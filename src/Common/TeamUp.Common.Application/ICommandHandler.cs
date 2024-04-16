using MediatR;

using RailwayResult;

using TeamUp.Application.Abstractions;

namespace TeamUp.Common.Application;

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result> where TCommand : ICommand
{
	public new Task<Result> Handle(TCommand command, CancellationToken ct);
};

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>> where TCommand : ICommand<TResponse>
{
	public new Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct);
};
