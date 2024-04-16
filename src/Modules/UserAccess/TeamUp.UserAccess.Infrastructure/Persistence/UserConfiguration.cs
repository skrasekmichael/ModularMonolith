using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Infrastructure.Persistence;

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
			.Property(user => user.Password)
			.IsRequired()
			.HasConversion(password => password.GetBytes(), bytes => new Password(bytes));

		userEntityBuilder
			.Property<uint>("RowVersion")
			.IsRowVersion();
	}
}
