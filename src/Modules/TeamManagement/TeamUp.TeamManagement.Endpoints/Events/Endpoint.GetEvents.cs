using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Events.GetEvents;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Endpoints.Events;

public sealed class GetEventsEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapGet("/", GetEventsAsync)
			.Produces<List<EventSlimResponse>>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(GetEventsEndpoint))
			.MapToApiVersion(1);
	}

	public async Task<IResult> GetEventsAsync(
		[FromRoute] Guid teamId,
		[FromQuery] DateTime? fromUtc,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var query = new GetEventsQuery
		{
			InitiatorId = httpContext.GetCurrentUserId(),
			TeamId = TeamId.FromGuid(teamId),
			FromUtc = fromUtc
		};

		var result = await sender.Send(query, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
