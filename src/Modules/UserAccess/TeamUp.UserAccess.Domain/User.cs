using TeamUp.Common.Domain;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Domain.DomainEvents;

namespace TeamUp.UserAccess.Domain;

public sealed class User : AggregateRoot<User, UserId>
{
	public string Name { get; private set; }
	public string Email { get; private set; }
	public Password Password { get; private set; }
	public UserState State { get; private set; }

#pragma warning disable CS8618 // EF Core constructor
	private User() : base() { }
#pragma warning restore CS8618

	private User(UserId id, string name, string email, Password password, UserState state) : base(id)
	{
		Name = name;
		Email = email;
		Password = password;
		State = state;

		AddDomainEvent(new UserCreatedDomainEvent(this));
	}

	internal static User Create(string name, string email, Password password) => new(
		UserId.New(),
		name,
		email,
		password,
		UserState.NotActivated
	);
}
