using TeamUp.TeamManagement.Contracts.Events;

namespace TeamUp.TeamManagement.Domain.Aggregates.Events;

public static class EventStatusExtensions
{
	public static bool IsOpenForResponses(this EventStatus status) => status == EventStatus.Open;
}
