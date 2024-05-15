using TeamUp.Common.Application;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.Activation;
using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Application;

internal sealed class ActivateAccountCommandHandler : ICommandHandler<ActivateAccountCommand>
{
	private readonly IUserRepository _userRepository;
	private readonly IUnitOfWork<UserAccessModuleId> _unitOfWork;

	public ActivateAccountCommandHandler(IUserRepository userRepository, IUnitOfWork<UserAccessModuleId> unitOfWork)
	{
		_userRepository = userRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(ActivateAccountCommand command, CancellationToken ct)
	{
		var user = await _userRepository.GetUserByIdAsync(command.UserId, ct);
		return await user
			.EnsureNotNull(UserErrors.UserNotFound)
			.Then(user => user.Activate())
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
