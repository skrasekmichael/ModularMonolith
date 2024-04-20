using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Infrastructure.Processing.Inbox;
using TeamUp.Common.Infrastructure.Processing.Outbox;

namespace TeamUp.Common.Infrastructure.Extensions;

public static class ModelBuilderExtensions
{
	public static void ConfigureOutbox(this ModelBuilder builder)
	{
		builder.Entity<OutboxMessage>()
			.ToTable("OutboxMessages")
			.HasKey(m => m.Id);

		builder.Entity<InboxMessage>()
			.ToTable("InboxMessages")
			.HasKey(m => m.Id);
	}
}
