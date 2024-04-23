using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Contracts.Teams.RemoveTeamMember;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Endpoints.Teams;

internal sealed class RemoveTeamMemberEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapDelete("/{teamId:guid}/members/{teamMemberId:guid}", RemoveTeamMemberAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(RemoveTeamMemberEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> RemoveTeamMemberAsync(
		[FromRoute] Guid teamId,
		[FromRoute] Guid teamMemberId,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new RemoveTeamMemberCommand
		{
			InitiatorId = httpContext.GetCurrentUserId(),
			TeamId = TeamId.FromGuid(teamId),
			MemberId = TeamMemberId.FromGuid(teamMemberId)
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
