using Microsoft.EntityFrameworkCore;

using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Domain.Aggregates.Events;

namespace TeamUp.TeamManagement.Infrastructure.Persistence.Domain.Events;

internal sealed class EventRepository : IEventRepository
{
	private readonly TeamManagementDbContext _context;

	public EventRepository(TeamManagementDbContext context)
	{
		_context = context;
	}

	public void AddEvent(Event @event) => _context.Events.Add(@event);

	public void RemoveEvent(Event @event) => _context.Remove(@event);

	public async Task<Event?> GetEventByIdAsync(EventId eventId, CancellationToken ct = default)
	{
		return await _context.Events
			.Include(e => e.EventResponses)
			.FirstOrDefaultAsync(e => e.Id == eventId, ct);
	}
}
