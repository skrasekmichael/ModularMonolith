using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Common.Contracts;
using TeamUp.Common.Domain;

namespace TeamUp.Common.Infrastructure.Persistence;

public abstract class BaseEntityConfiguration<TEntity, TId> : IEntityTypeConfiguration<TEntity>
	where TEntity : Entity<TId>
	where TId : TypedId<TId>, new()
{
	public void Configure(EntityTypeBuilder<TEntity> builder)
	{
		builder.HasKey(entity => entity.Id);

		builder.Property(entity => entity.Id)
			.HasConversion(typedId => typedId.Value, guid => TypedId<TId>.FromGuid(guid));

		ConfigureEntity(builder);
	}

	protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}
