using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.UserAccess.Domain;

public sealed class UserFactory
{
	private readonly IUserRepository _userRepository;

	public UserFactory(IUserRepository userRepository)
	{
		_userRepository = userRepository;
	}

	public async Task<Result<User>> CreateAndAddUserAsync(string name, string email, Password password, CancellationToken ct = default)
	{
		return await name
			.Ensure(Rules.UserNameMinSize, Rules.UserNameMaxSize)
			.ThenAsync(_ => _userRepository.ExistsUserWithConflictingEmailAsync(email, ct))
			.Ensure(conflictingUserExists => conflictingUserExists == false, Errors.ConflictingEmail)
			.Then(_ => User.Create(name, email, password))
			.Tap(_userRepository.AddUser);
	}
}
