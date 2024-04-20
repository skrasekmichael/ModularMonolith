using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using RailwayResult;
using RailwayResult.Errors;

using TeamUp.Common.Contracts.Errors;

using IResult = Microsoft.AspNetCore.Http.IResult;

namespace TeamUp.Common.Endpoints;

public static class ResultToResponseExtensions
{
	public static IResult ToResponse<TOut>(this Result<TOut> result, Func<TOut, IResult> success)
	{
		if (result.IsSuccess)
			return success(result.Value);

		return result.Error.ToResponse();
	}

	public static IResult ToResponse(this Result result, Func<IResult> success)
	{
		if (result.IsSuccess)
			return success();

		return result.Error.ToResponse();
	}

	public static IResult ToResponse(this Error error)
	{
		if (error is ValidationErrors validationErrors)
		{
			return TypedResults.Problem(new ValidationProblemDetails
			{
				Title = validationErrors.GetType().Name,
				Detail = validationErrors.Message,
				Status = StatusCodes.Status400BadRequest,
				Errors = validationErrors.Errors
			});
		}

		return TypedResults.Problem(new ProblemDetails
		{
			Title = error.GetType().Name,
			Detail = error.Message,
			Status = error switch
			{
				AuthenticationError => StatusCodes.Status401Unauthorized,
				AuthorizationError => StatusCodes.Status403Forbidden,
				NotFoundError => StatusCodes.Status404NotFound,
				ValidationError => StatusCodes.Status400BadRequest,
				DomainError => StatusCodes.Status400BadRequest,
				ConflictError => StatusCodes.Status409Conflict,
				ConcurrencyError => StatusCodes.Status409Conflict,

				InternalError => StatusCodes.Status500InternalServerError,
				_ => StatusCodes.Status500InternalServerError
			}
		});
	}
}
