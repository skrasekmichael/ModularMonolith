using TeamUp.Common.Contracts;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.UserAccess.Domain;

public sealed class UserFactory
{
	private readonly IUserRepository _userRepository;
	private readonly IDateTimeProvider _dateTimeProvider;

	public UserFactory(IUserRepository userRepository, IDateTimeProvider dateTimeProvider)
	{
		_userRepository = userRepository;
		_dateTimeProvider = dateTimeProvider;
	}

	public async Task<Result<User>> CreateAndAddUserAsync(string name, string email, Password password, CancellationToken ct = default)
	{
		return await name
			.Ensure(Rules.UserNameMinSize, Rules.UserNameMaxSize)
			.ThenAsync(_ => _userRepository.ExistsUserWithConflictingEmailAsync(email, ct))
			.Ensure(conflictingUserExists => conflictingUserExists == false, UserErrors.ConflictingEmail)
			.Then(_ => new User(UserId.New(), name, email, password, UserState.NotActivated, _dateTimeProvider.UtcNow))
			.Tap(_userRepository.AddUser);
	}

	public async Task<Result<User>> GenerateAndAddUserAsync(string name, string email, CancellationToken ct = default)
	{
		return await name
			.Ensure(Rules.UserNameMinSize, Rules.UserNameMaxSize)
			.ThenAsync(_ => _userRepository.ExistsUserWithConflictingEmailAsync(email, ct))
			.Ensure(conflictingUserExists => conflictingUserExists == false, UserErrors.ConflictingEmail)
			.Then(_ => new User(UserId.New(), name, email, new Password(), UserState.Generated, _dateTimeProvider.UtcNow))
			.Tap(_userRepository.AddUser);
	}
}
