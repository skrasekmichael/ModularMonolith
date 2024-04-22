using Microsoft.EntityFrameworkCore;

using TeamUp.TeamManagement.Domain.Aggregates.Users;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Infrastructure.Persistence.Domain.Users;

internal sealed class UserRepository : IUserRepository
{
	private readonly TeamManagementDbContext _context;

	public UserRepository(TeamManagementDbContext context)
	{
		_context = context;
	}

	public void AddUser(User user) => _context.Users.Add(user);

	public void RemoveUser(User user) => _context.Users.Remove(user);

	public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
	{
		return await _context.Users.FirstOrDefaultAsync(user => user.Email == email, ct);
	}

	public async Task<User?> GetUserByIdAsync(UserId id, CancellationToken ct = default)
	{
		return await _context.Users.FindAsync([id], ct);
	}
}
