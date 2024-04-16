using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.CreateUser;

namespace TeamUp.UserAccess.Endpoints;

internal sealed class RegisterUserEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapPost("/register", RegisterUserAsync)
			.Produces<UserId>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status409Conflict)
			.ProducesValidationProblem()
			.WithName(nameof(RegisterUserEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> RegisterUserAsync(
		[FromBody] RegisterUserRequest request,
		[FromServices] ISender sender,
		[FromServices] LinkGenerator linkGenerator,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new RegisterUserCommand
		{
			Email = request.Email,
			Name = request.Name,
			Password = request.Password
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(userId => TypedResults.Created(
			uri: linkGenerator.GetPathByName(httpContext, nameof(GetMyAccountEndpoint)),
			value: userId
		));
	}
}
