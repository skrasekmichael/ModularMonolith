using RailwayResult.Errors;

using TeamUp.TeamManagement.Contracts.Events;

namespace TeamUp.TeamManagement.Domain.Aggregates.Events;

public static class EventErrors
{
	public static readonly ValidationError EventDescriptionMaxSize = new("TeamManagement.Events.Validation.DescriptionMaxSize", $"Event's description must be shorter than {EventConstants.EVENT_DESCRIPTION_MAX_SIZE} characters.");

	public static readonly ValidationError CannotEndBeforeStart = new("TeamManagement.Events.Validation.StartBeforeEnd", "Event cannot end before it starts.");
	public static readonly ValidationError CannotStartInPast = new("TeamManagement.Events.Validation.StartInPast", "Cannot create event in the past.");

	public static readonly DomainError NotOpenForResponses = new("TeamManagement.Events.Domain.ClosedForResponses", "Event is not open for responses.");
	public static readonly DomainError TimeForResponsesExpired = new("TeamManagement.Events.Domain.RespondTimeExpired", "Time for responses expired.");

	public static readonly NotFoundError EventNotFound = new("TeamManagement.Events.NotFound", "Event not found.");
}
