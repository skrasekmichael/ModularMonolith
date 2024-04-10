using MediatR;

namespace TeamUp.Application.Abstractions;

public interface ICommand<TResponse> : IRequest<TResponse>;
