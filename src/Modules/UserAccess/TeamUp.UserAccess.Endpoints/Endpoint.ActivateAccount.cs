using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.Activation;

namespace TeamUp.UserAccess.Endpoints;

internal sealed class ActivateAccountEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapPost("/{userId:guid}/activate", ActivateAccountAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(ActivateAccountEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> ActivateAccountAsync(
		[FromRoute] Guid userId,
		[FromServices] ISender sender,
		CancellationToken ct)
	{
		var command = new ActivateAccountCommand
		{
			UserId = UserId.FromGuid(userId)
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
