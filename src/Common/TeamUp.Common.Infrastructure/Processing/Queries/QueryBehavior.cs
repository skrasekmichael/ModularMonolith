using System.Runtime.CompilerServices;

using FluentValidation;
using FluentValidation.Results;

using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using RailwayResult;

using TeamUp.Common.Contracts;
using TeamUp.Common.Contracts.Errors;

namespace TeamUp.Common.Infrastructure.Processing.Queries;

internal sealed class QueryBehavior<TQuery, TResponse> : IPipelineBehavior<TQuery, TResponse>
	where TQuery : IQueryPipeline
	where TResponse : class, IResult
{
	private static readonly string ValidationErrorsKey = $"{typeof(TQuery).Name}.ValidationErrors";

	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<QueryBehavior<TQuery, TResponse>> _logger;

	public QueryBehavior(IServiceProvider serviceProvider, ILogger<QueryBehavior<TQuery, TResponse>> logger)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
	}

	public static ValidationErrors ValidationError(ValidationResult validationResult) =>
		new(ValidationErrorsKey, "One or more validation errors occurred.", validationResult.ToDictionary());

	public async Task<TResponse> Handle(TQuery query, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
	{
		_logger.LogInformation("Behavior for query {query} executed.", typeof(TQuery).Name);

		var validator = _serviceProvider.GetService<IValidator<TQuery>>();
		if (validator is not null)
		{
			var validationResult = await validator.ValidateAsync(query, ct);
			if (!validationResult.IsValid)
			{
				return Unsafe.As<TResponse>(TResponse.FromError(ValidationError(validationResult)));
			}
		}

		return await next();
	}
}
