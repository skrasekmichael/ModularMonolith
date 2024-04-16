using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Application.Abstractions;

public interface IUserAccessQueryContext
{
	public IQueryable<User> Users { get; }
}
