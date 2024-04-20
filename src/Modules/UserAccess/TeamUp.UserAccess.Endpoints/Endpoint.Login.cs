using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.UserAccess.Contracts.Login;

namespace TeamUp.UserAccess.Endpoints;

internal sealed class LoginEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapPost("/login", LoginAsync)
			.Produces<string>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesValidationProblem()
			.WithName(nameof(LoginEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> LoginAsync(
		[FromBody] LoginRequest request,
		[FromServices] ISender sender,
		CancellationToken ct)
	{
		var command = new LoginCommand
		{
			Email = request.Email,
			Password = request.Password
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
