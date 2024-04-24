using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Invitations.AcceptInvitation;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Endpoints.Invitations;

public sealed class AcceptInvitationEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapPost("/{invitationId:guid}/accept", AcceptInvitationAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(AcceptInvitationEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> AcceptInvitationAsync(
		[FromRoute] Guid invitationId,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new AcceptInvitationCommand
		{
			InitiatorId = httpContext.GetCurrentUserId(),
			InvitationId = InvitationId.FromGuid(invitationId)
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
