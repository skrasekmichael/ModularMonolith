using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Domain.Aggregates.Events;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;

namespace TeamUp.TeamManagement.Infrastructure.Persistence.Domain.Events;

internal sealed class EventResponseConfiguration : BaseEntityConfiguration<EventResponse, EventResponseId>
{
	protected override void ConfigureEntity(EntityTypeBuilder<EventResponse> eventResponseEntityBuilder)
	{
		// Since there is no navigation property from EventResponse to Event, specifying would lead to key duplicity
		//eventResponseEntityBuilder
		//	.HasOne<Event>()
		//	.WithMany()
		//	.HasForeignKey(eventResponse => eventResponse.EventId);

		eventResponseEntityBuilder
			.HasOne<TeamMember>()
			.WithMany()
			.HasForeignKey(eventResponse => eventResponse.TeamMemberId);

		eventResponseEntityBuilder
			.Property(eventResponse => eventResponse.Message)
			.HasMaxLength(255);

		eventResponseEntityBuilder
			.HasIndex(eventResponse => eventResponse.EventId);

		eventResponseEntityBuilder
			.HasIndex(eventResponse => new { eventResponse.EventId, eventResponse.TeamMemberId })
			.IsUnique();
	}
}
