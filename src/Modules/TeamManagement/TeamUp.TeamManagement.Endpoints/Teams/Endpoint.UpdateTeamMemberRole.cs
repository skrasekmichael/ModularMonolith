using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Contracts.Teams.SetMemberRole;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Endpoints.Teams;

internal sealed class UpdateTeamMemberRoleEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapPut("/{teamId:guid}/members/{teamMemberId:guid}/role", UpdateTeamRoleAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(UpdateTeamMemberRoleEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> UpdateTeamRoleAsync(
		[FromRoute] Guid teamId,
		[FromRoute] Guid teamMemberId,
		[FromBody] UpdateTeamRoleRequest request,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new SetMemberRoleCommand
		{
			InitiatorId = httpContext.GetCurrentUserId(),
			TeamId = TeamId.FromGuid(teamId),
			MemberId = TeamMemberId.FromGuid(teamMemberId),
			Role = request.Role
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
