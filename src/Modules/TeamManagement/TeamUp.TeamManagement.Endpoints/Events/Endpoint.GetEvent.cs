using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Events.GetEvent;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Endpoints.Events;

public sealed class GetEventEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapGet("/{eventId:guid}", GetEventAsync)
			.Produces<EventResponse>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(GetEventEndpoint))
			.MapToApiVersion(1);
	}

	public async Task<IResult> GetEventAsync(
		[FromRoute] Guid teamId,
		[FromRoute] Guid eventId,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var query = new GetEventQuery
		{
			InitiatorId = httpContext.GetCurrentUserId(),
			TeamId = TeamId.FromGuid(teamId),
			EventId = EventId.FromGuid(eventId)
		};

		var result = await sender.Send(query, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
