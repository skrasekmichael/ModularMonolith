using TeamUp.TeamManagement.Contracts.Events;

namespace TeamUp.TeamManagement.Domain.Aggregates.Events;

public interface IEventRepository
{
	public Task<Event?> GetEventByIdAsync(EventId eventId, CancellationToken ct = default);
	public void AddEvent(Event @event);
	public void RemoveEvent(Event @event);
}
