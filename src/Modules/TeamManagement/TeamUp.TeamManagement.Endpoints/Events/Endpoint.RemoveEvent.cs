using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Events.RemoveEvent;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Endpoints.Events;

public sealed class RemoveEventEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapDelete("/{eventId:guid}", RemoveEventAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(RemoveEventEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> RemoveEventAsync(
		[FromRoute] Guid teamId,
		[FromRoute] Guid eventId,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new RemoveEventCommand
		{
			InitiatorId = httpContext.GetCurrentUserId(),
			TeamId = TeamId.FromGuid(teamId),
			EventId = EventId.FromGuid(eventId)
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
