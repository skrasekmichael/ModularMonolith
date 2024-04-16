using TeamUp.Common.Domain;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Domain.Aggregates;

public sealed class User : AggregateRoot<User, UserId>
{
	public string Name { get; private set; }
	public string Email { get; private set; }
	public int NumberOfOwnedTeams { get; private set; }

#pragma warning disable CS8618 // EF Core constructor
	private User() : base() { }
#pragma warning restore CS8618

	public User(UserId id, string name, string email) : base(id)
	{
		Name = name;
		Email = email;
		NumberOfOwnedTeams = 0;
	}
}
