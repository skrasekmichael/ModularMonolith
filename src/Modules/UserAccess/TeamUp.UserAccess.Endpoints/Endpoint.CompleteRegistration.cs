using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.CompleteRegistration;

namespace TeamUp.UserAccess.Endpoints;

public sealed class CompleteRegistrationEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapPost("/{userId:guid}/generated/complete", ActivateAccountAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(CompleteRegistrationEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> ActivateAccountAsync(
		[FromRoute] Guid userId,
		[FromHeader(Name = UserConstants.HTTP_HEADER_PASSWORD)] string password,
		[FromServices] ISender sender,
		CancellationToken ct)
	{
		var command = new CompleteRegistrationCommand
		{
			UserId = UserId.FromGuid(userId),
			Password = password
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
