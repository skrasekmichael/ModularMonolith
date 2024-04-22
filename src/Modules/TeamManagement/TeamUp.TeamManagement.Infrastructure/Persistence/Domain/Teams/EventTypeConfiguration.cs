using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Events;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;

namespace TeamUp.TeamManagement.Infrastructure.Persistence.Domain.Teams;

internal sealed class EventTypeConfiguration : BaseEntityConfiguration<EventType, EventTypeId>
{
	protected override void ConfigureEntity(EntityTypeBuilder<EventType> eventTypeEntityBuilder)
	{
		eventTypeEntityBuilder
			.HasOne<Team>()
			.WithMany()
			.HasForeignKey(eventType => eventType.TeamId);

		eventTypeEntityBuilder
			.HasMany<Event>()
			.WithOne()
			.HasForeignKey(e => e.EventTypeId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
