using MediatR;

using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Domain;
using TeamUp.Common.Infrastructure.Persistence;

namespace TeamUp.Common.Infrastructure.Processing;

internal sealed class DomainEventsDispatcher
{
	private readonly IPublisher _publisher;

	public DomainEventsDispatcher(IPublisher publisher)
	{
		_publisher = publisher;
	}

	public async Task DispatchDomainEventsAsync<TDatabaseContext>(TDatabaseContext dbContext, CancellationToken ct = default) where TDatabaseContext : DbContext, IDatabaseContext
	{
		List<IHasDomainEvent> GetEntitiesWithUnpublishedDomainEvents()
		{
			//get all entities with unpublished domain events
			return dbContext.ChangeTracker
				.Entries<IHasDomainEvent>()
				.Where(entry => entry.Entity.DomainEvents.Any())
				.Select(entry => entry.Entity)
				.ToList();
		}

		List<IHasDomainEvent> entities;

		while ((entities = GetEntitiesWithUnpublishedDomainEvents()).Count != 0)
		{
			//get all unpublished domain events
			var domainEvents = entities.SelectMany(entity => entity.DomainEvents).ToList();

			//clear all domain events
			entities.ForEach(entity => entity.ClearDomainEvents());

			//publish all domain events
			var tasks = domainEvents.Select(domainEvent => _publisher.Publish(domainEvent, ct));
			await Task.WhenAll(tasks);
		}
	}
}
