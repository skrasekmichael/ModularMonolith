using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Invitations.GetMyInvitations;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Endpoints.Invitations;

public sealed class GetMyInvitationsEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapGet("/", GetTeamInvitationsAsync)
			.Produces<List<InvitationResponse>>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.WithName(nameof(GetMyInvitationsEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> GetTeamInvitationsAsync(
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var query = new GetMyInvitationsQuery
		{
			InitiatorId = httpContext.GetCurrentUserId()
		};

		var result = await sender.Send(query, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
