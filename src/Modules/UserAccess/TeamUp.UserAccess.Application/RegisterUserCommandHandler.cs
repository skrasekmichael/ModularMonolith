using TeamUp.Common.Application;
using TeamUp.UserAccess.Application.Abstractions;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.CreateUser;
using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Application;

internal sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, UserId>
{
	private readonly UserFactory _userFactory;
	private readonly IPasswordService _passwordService;
	private readonly IUnitOfWork<UserAccessModuleId> _unitOfWork;

	public RegisterUserCommandHandler(
		UserFactory userFactory,
		IPasswordService passwordService,
		IUnitOfWork<UserAccessModuleId> unitOfWork)
	{
		_userFactory = userFactory;
		_passwordService = passwordService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<UserId>> Handle(RegisterUserCommand command, CancellationToken ct)
	{
		var password = _passwordService.HashPassword(command.Password);
		var user = await _userFactory.CreateAndAddUserAsync(command.Name, command.Email, password, ct);

		return await user
			.Then(user => user.Id)
			.TapAsync(_ => _unitOfWork.SaveChangesAsync(ct));
	}
}
