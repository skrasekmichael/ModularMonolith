using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Domain.Aggregates.Events;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;

namespace TeamUp.TeamManagement.Infrastructure.Persistence.Domain.Events;

internal sealed class EventConfiguration : BaseEntityConfiguration<Event, EventId>
{
	protected override void ConfigureEntity(EntityTypeBuilder<Event> eventEntityBuilder)
	{
		eventEntityBuilder
			.HasOne<Team>()
			.WithMany()
			.HasForeignKey(e => e.TeamId);

		eventEntityBuilder
			.HasOne<EventType>()
			.WithMany()
			.HasForeignKey(e => e.EventTypeId);

		eventEntityBuilder
			.HasMany(e => e.EventResponses)
			.WithOne()
			.HasForeignKey(eventResponse => eventResponse.EventId)
			.OnDelete(DeleteBehavior.Cascade);

		eventEntityBuilder
			.HasIndex(e => e.TeamId);

		eventEntityBuilder
			.HasIndex(e => e.EventTypeId);

		eventEntityBuilder
			.Property<uint>("RowVersion")
			.IsRowVersion();
	}
}
