using TeamUp.Application.Abstractions;

namespace TeamUp.Common.Application;

public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>;
