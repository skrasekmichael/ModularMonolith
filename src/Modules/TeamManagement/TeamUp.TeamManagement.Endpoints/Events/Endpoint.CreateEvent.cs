using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Events.CreateEvent;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Endpoints.Events;

public sealed class CreateEventEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapPost("/", CreateEventAsync)
			.Produces<EventId>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(CreateEventEndpoint))
			.MapToApiVersion(1);
	}

	public async Task<IResult> CreateEventAsync(
		[FromRoute] Guid teamId,
		[FromBody] CreateEventRequest request,
		[FromServices] ISender sender,
		[FromServices] LinkGenerator linkGenerator,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new CreateEventCommand
		{
			InitiatorId = httpContext.GetCurrentUserId(),
			TeamId = TeamId.FromGuid(teamId),
			EventTypeId = request.EventTypeId,
			FromUtc = request.FromUtc,
			ToUtc = request.ToUtc,
			Description = request.Description,
			MeetTime = request.MeetTime,
			ReplyClosingTimeBeforeMeetTime = request.ReplyClosingTimeBeforeMeetTime
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(eventId => TypedResults.Created(
			linkGenerator.GetPathByName(httpContext, nameof(GetEventEndpoint), new { teamId, eventId = eventId.Value }),
			eventId
		));
	}
}
