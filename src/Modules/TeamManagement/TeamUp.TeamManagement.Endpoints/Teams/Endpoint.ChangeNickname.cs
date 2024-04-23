using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Contracts.Teams.ChangeNickname;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Endpoints.Teams;

internal sealed class ChangeNicknameEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapPut("/{teamId:guid}/nickname", ChangeNicknameAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(ChangeNicknameEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> ChangeNicknameAsync(
		[FromRoute] Guid teamId,
		[FromBody] ChangeNicknameRequest request,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new ChangeNicknameCommand
		{
			InitiatorId = httpContext.GetCurrentUserId(),
			TeamId = TeamId.FromGuid(teamId),
			Nickname = request.Nickname
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
