using TeamUp.Common.Application;
using TeamUp.UserAccess.Application.Abstractions;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.CompleteRegistration;
using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Application;

internal sealed class CompleteRegistrationCommandHandler : ICommandHandler<CompleteRegistrationCommand>
{
	private readonly IUserRepository _userRepository;
	private readonly IPasswordService _passwordService;
	private readonly IUnitOfWork<UserAccessModuleId> _unitOfWork;

	public CompleteRegistrationCommandHandler(IUserRepository userRepository, IPasswordService passwordService, IUnitOfWork<UserAccessModuleId> unitOfWork)
	{
		_userRepository = userRepository;
		_passwordService = passwordService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(CompleteRegistrationCommand command, CancellationToken ct)
	{
		var user = await _userRepository.GetUserByIdAsync(command.UserId, ct);
		return await user
			.EnsureNotNull(UserErrors.UserNotFound)
			.Then(user => user.CompleteGeneratedRegistration(_passwordService.HashPassword(command.Password)))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
