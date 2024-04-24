using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Invitations.RemoveInvitation;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Endpoints.Invitations;

public sealed class RemoveInvitationEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapDelete("/{invitationId:guid}", RemoveInvitationAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(RemoveInvitationEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> RemoveInvitationAsync(
		[FromRoute] Guid invitationId,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new RemoveInvitationCommand
		{
			InitiatorId = httpContext.GetCurrentUserId(),
			InvitationId = InvitationId.FromGuid(invitationId)
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
