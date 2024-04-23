using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Contracts.Teams.DeleteTeam;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Endpoints.Teams;

internal sealed class DeleteTeamEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapDelete("/{teamId:guid}", DeleteTeamAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(DeleteTeamEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> DeleteTeamAsync(
		[FromRoute] Guid teamId,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new DeleteTeamCommand
		{
			InitiatorId = httpContext.GetCurrentUserId(),
			TeamId = TeamId.FromGuid(teamId)
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
