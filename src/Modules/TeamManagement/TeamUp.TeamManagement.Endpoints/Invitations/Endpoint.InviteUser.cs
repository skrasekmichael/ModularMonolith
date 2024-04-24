using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Invitations.InviteUser;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Endpoints.Invitations;

public sealed class InviteUserEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapPost("/", InviteUserAsync)
			.Produces<InvitationId>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(InviteUserEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> InviteUserAsync(
		[FromBody] InviteUserRequest request,
		[FromServices] ISender sender,
		[FromServices] LinkGenerator linkGenerator,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new InviteUserCommand
		{
			InitiatorId = httpContext.GetCurrentUserId(),
			TeamId = request.TeamId,
			Email = request.Email
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(() => TypedResults.Accepted(
			uri: linkGenerator.GetPathByName(httpContext, nameof(GetTeamInvitationsEndpoint), request.TeamId.Value)
		));
	}
}
