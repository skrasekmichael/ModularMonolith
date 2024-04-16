using Microsoft.EntityFrameworkCore;

using TeamUp.Domain.Aggregates.Users;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Infrastructure.Persistence;

internal sealed class UserRepository : IUserRepository
{
	private readonly UserAccessDbContext _context;

	public UserRepository(UserAccessDbContext context)
	{
		_context = context;
	}

	public void AddUser(User user) => _context.Users.Add(user);

	public void RemoveUser(User user) => _context.Users.Remove(user);

	public async Task<bool> ExistsUserWithConflictingEmailAsync(string email, CancellationToken ct = default)
	{
		return await _context.Users.AnyAsync(user => user.Email == email, ct);
	}

	public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
	{
		return await _context.Users.FirstOrDefaultAsync(user => user.Email == email, ct);
	}

	public async Task<User?> GetUserByIdAsync(UserId id, CancellationToken ct = default)
	{
		return await _context.Users.FindAsync([id], ct);
	}
}
