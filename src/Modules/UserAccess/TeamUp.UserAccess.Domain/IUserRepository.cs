﻿using TeamUp.UserAccess.Contracts;

namespace TeamUp.UserAccess.Domain;

public interface IUserRepository
{
	public Task<User?> GetUserByIdAsync(UserId id, CancellationToken ct = default);
	public Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default);
	public Task<bool> ExistsUserWithConflictingEmailAsync(string email, CancellationToken ct = default);
	public void AddUser(User user);
	public void RemoveUser(User user);
}
