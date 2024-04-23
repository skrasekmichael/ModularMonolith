using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Contracts.Teams.CreateEventType;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Endpoints.Teams;

internal sealed class CreateEventTypeEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group)
	{
		group.MapPost("/{teamId:guid}/event-types", CreateEventTypeAsync)
			.Produces<EventTypeId>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(CreateEventTypeEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> CreateEventTypeAsync(
		[FromRoute] Guid teamId,
		[FromBody] UpsertEventTypeRequest request,
		[FromServices] ISender sender,
		[FromServices] LinkGenerator linkGenerator,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new CreateEventTypeCommand
		{
			InitiatorId = httpContext.GetCurrentUserId(),
			TeamId = TeamId.FromGuid(teamId),
			Name = request.Name,
			Description = request.Description
		};

		var result = await sender.Send(command, ct);
		return result.ToResponse(eventTypeId => TypedResults.Created(
			uri: linkGenerator.GetPathByName(httpContext, nameof(GetTeamEndpoint), teamId),
			value: eventTypeId
		));
	}
}
