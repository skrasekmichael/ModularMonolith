using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.TeamManagement.Domain.Aggregates.Users;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Infrastructure.Persistence.Domain.Users;

internal sealed class UserConfiguration : BaseEntityConfiguration<User, UserId>
{
	protected override void ConfigureEntity(EntityTypeBuilder<User> userEntityBuilder)
	{
		userEntityBuilder
			.HasIndex(user => user.Email)
			.IsUnique();

		userEntityBuilder
			.Property(user => user.Email)
			.IsRequired()
			.HasMaxLength(255);

		userEntityBuilder
			.Property(user => user.Name)
			.IsRequired()
			.HasMaxLength(255);

		userEntityBuilder
			.Property<uint>("RowVersion")
			.IsRowVersion();
	}
}
