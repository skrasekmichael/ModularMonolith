using TeamUp.Application.Abstractions;

namespace TeamUp.Common.Application;

public interface ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>;
