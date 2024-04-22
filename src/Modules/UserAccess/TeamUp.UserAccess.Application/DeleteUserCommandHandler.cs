using TeamUp.Common.Application;
using TeamUp.UserAccess.Application.Abstractions;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.DeleteUser;
using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Application;

internal sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
	private readonly IUserRepository _userRepository;
	private readonly IPasswordService _passwordService;
	private readonly IUnitOfWork<UserAccessModuleId> _unitOfWork;

	public DeleteUserCommandHandler(IUserRepository userRepository, IPasswordService passwordService, IUnitOfWork<UserAccessModuleId> unitOfWork)
	{
		_userRepository = userRepository;
		_passwordService = passwordService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(DeleteUserCommand command, CancellationToken ct)
	{
		var user = await _userRepository.GetUserByIdAsync(command.InitiatorId, ct);
		return await user
			.EnsureNotNull(UserErrors.UserNotFound)
			.Ensure(user => _passwordService.VerifyPassword(command.Password, user.Password), AuthenticationErrors.InvalidCredentials)
			.Tap(user => user.Delete())
			.TapAsync(_ => _unitOfWork.SaveChangesAsync(ct))
			.ToResultAsync();
	}
}
