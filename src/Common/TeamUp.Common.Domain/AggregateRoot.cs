using TeamUp.Common.Contracts;

namespace TeamUp.Common.Domain;

public abstract class AggregateRoot<TSelf, TId> : Entity<TId>
	where TSelf : AggregateRoot<TSelf, TId>
	where TId : TypedId<TId>, new()
{
	protected internal AggregateRoot() : base() { }

	protected AggregateRoot(TId id) : base(id) { }
}
