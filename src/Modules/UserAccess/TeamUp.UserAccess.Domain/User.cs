using System.Linq.Expressions;

using TeamUp.Common.Domain;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Domain.DomainEvents;

namespace TeamUp.UserAccess.Domain;

public sealed class User : AggregateRoot<User, UserId>
{
	internal static readonly TimeSpan GeneratedUserTTL = TimeSpan.FromDays(2);
	internal static readonly TimeSpan RegisteredUserTTL = TimeSpan.FromDays(1);

	public string Name { get; private set; }
	public string Email { get; private set; }
	public Password Password { get; private set; }
	public UserState State { get; private set; }
	public DateTime CreatedUtc { get; private set; }

#pragma warning disable CS8618 // EF Core constructor
	private User() : base() { }
#pragma warning restore CS8618

	internal User(UserId id, string name, string email, Password password, UserState state, DateTime utcNow) : base(id)
	{
		Name = name;
		Email = email;
		Password = password;
		State = state;
		CreatedUtc = utcNow;

		AddDomainEvent(new UserCreatedDomainEvent(this));
	}

	public void Delete()
	{
		AddDomainEvent(new UserDeletedDomainEvent(this));
	}

	public void Activate()
	{
		State = UserState.Activated;
		AddDomainEvent(new UserActivatedDomainEvent(this));
	}

	internal static Expression<Func<User, bool>> AccountHasExpiredExpression(DateTime utcNow)
	{
		return user =>
			(user.State == UserState.Generated && (utcNow - user.CreatedUtc) > GeneratedUserTTL) ||
			(user.State == UserState.NotActivated && (utcNow - user.CreatedUtc) > RegisteredUserTTL);
	}
}
