using System.Runtime.CompilerServices;
using System.Transactions;

using FluentValidation;
using FluentValidation.Results;

using MediatR;

using Microsoft.Extensions.Logging;

using RailwayResult;
using RailwayResult.Errors;

using TeamUp.Application.Abstractions;
using TeamUp.Common.Contracts.Errors;
using TeamUp.Common.Infrastructure.Persistence;

namespace TeamUp.Common.Infrastructure.Processing.Commands;

internal sealed class CommandBehavior<TCommand, TResponse> : IPipelineBehavior<TCommand, TResponse>
	where TCommand : ICommandPipeline
	where TResponse : class, IResult
{
	private static readonly string ValidationErrorsKey = $"{typeof(TCommand).Name}.ValidationErrors";

	private readonly IValidator<TCommand> _validator;
	private readonly ILogger<CommandBehavior<TCommand, TResponse>> _logger;
	private readonly OutboxDbContext _outboxDbContext;

	public CommandBehavior(IValidator<TCommand> validator, ILogger<CommandBehavior<TCommand, TResponse>> logger, OutboxDbContext outboxDbContext)
	{
		_validator = validator;
		_logger = logger;
		_outboxDbContext = outboxDbContext;
	}

	public static ValidationErrors ValidationError(ValidationResult validationResult) =>
		 new(ValidationErrorsKey, "One or more validation errors occurred.", validationResult.ToDictionary());

	public async Task<TResponse> Handle(TCommand command, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
	{
		_logger.LogInformation("Behavior for command {command} executed.", typeof(TCommand).Name);

		var validationResult = await _validator.ValidateAsync(command, ct);
		if (!validationResult.IsValid)
		{
			return Unsafe.As<TResponse>(TResponse.FromError(ValidationError(validationResult)));
		}

		using var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
		try
		{
			var result = await next();
			if (result.IsSuccess)
			{
				await _outboxDbContext.SaveChangesAsync(ct);
			}

			scope.Complete();
			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to save changes.");
			return Unsafe.As<TResponse>(TResponse.FromError(new InternalError("CommandPipeline.InternalError", "Failed to save changes.")));
		}
	}
}
