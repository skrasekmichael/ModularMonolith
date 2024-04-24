using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Invitations.GetTeamInvitations;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Endpoints.Invitations;

public sealed class GetTeamInvitationsEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapGet("/teams/{teamId:guid}", GetTeamInvitationsAsync)
			.Produces<List<TeamInvitationResponse>>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(GetTeamInvitationsEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> GetTeamInvitationsAsync(
		[FromRoute] Guid teamId,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var query = new GetTeamInvitationsQuery
		{
			InitiatorId = httpContext.GetCurrentUserId(),
			TeamId = TeamId.FromGuid(teamId)
		};

		var result = await sender.Send(query, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
