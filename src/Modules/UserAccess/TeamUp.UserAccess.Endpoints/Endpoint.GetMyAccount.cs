using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.GetAccountDetails;

namespace TeamUp.UserAccess.Endpoints;

internal sealed class GetMyAccountEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapGet("/", GetAccountDetailsAsync)
			.Produces<AccountResponse>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.WithName(nameof(GetMyAccountEndpoint))
			.MapToApiVersion(1)
			.RequireAuthorization();
	}

	private async Task<IResult> GetAccountDetailsAsync([FromServices] ISender sender, HttpContext httpContext, CancellationToken ct)
	{
		var query = new GetAccountDetailsQuery
		{
			UserId = httpContext.GetCurrentUserId<UserId>()
		};

		var result = await sender.Send(query, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
