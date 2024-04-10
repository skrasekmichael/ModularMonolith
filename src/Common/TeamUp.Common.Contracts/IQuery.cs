using MediatR;

namespace TeamUp.Application.Abstractions;

public interface IQuery<TResponse> : IRequest<TResponse>;
