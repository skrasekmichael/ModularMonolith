using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Domain.Aggregates.Users;

public interface IUserRepository
{
	public Task<User?> GetUserByIdAsync(UserId id, CancellationToken ct = default);
	public Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default);
	public void AddUser(User user);
	public void RemoveUser(User user);
}
