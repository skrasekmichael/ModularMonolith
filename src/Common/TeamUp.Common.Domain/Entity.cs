using TeamUp.Common.Contracts;

namespace TeamUp.Common.Domain;

public abstract class Entity<TId> : IEquatable<Entity<TId>>, IHasDomainEvent where TId : TypedId<TId>, new()
{
	private readonly List<IDomainEvent> _domainEvents = [];

	public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

	public TId Id { get; private init; }

#pragma warning disable CS8618 // EF Core constructor
	protected internal Entity() : base() { }
#pragma warning restore CS8618

	protected Entity(TId id)
	{
		Id = id;
	}

	protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

	public void ClearDomainEvents() => _domainEvents.Clear();

	public bool Equals(Entity<TId>? other) => other is not null && Id.Equals(other.Id);

	public override bool Equals(object? obj) => obj is Entity<TId> other && Id.Equals(other.Id);

	public override int GetHashCode() => Id.Value.GetHashCode();
}
