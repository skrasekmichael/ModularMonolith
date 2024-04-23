using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.DeleteUser;

namespace TeamUp.UserAccess.Endpoints;

internal sealed class DeleteUserEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapDelete("/", DeleteUserAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(DeleteUserEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> DeleteUserAsync(
		[FromServices] ISender sender,
		[FromHeader(Name = UserConstants.HTTP_HEADER_CONFIRM_PASSWORD)] string password,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new DeleteUserCommand
		{
			InitiatorId = httpContext.GetCurrentUserId(),
			Password = password
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
