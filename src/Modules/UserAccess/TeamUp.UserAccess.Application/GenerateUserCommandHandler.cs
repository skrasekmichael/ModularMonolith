using TeamUp.Common.Application;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.CreateUser;
using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Application;

internal sealed class GenerateUserCommandHandler : ICommandHandler<GenerateUserCommand, UserId>
{
	private readonly UserFactory _userFactory;
	private readonly IUnitOfWork<UserAccessModuleId> _unitOfWork;

	public GenerateUserCommandHandler(
		UserFactory userFactory,
		IUnitOfWork<UserAccessModuleId> unitOfWork)
	{
		_userFactory = userFactory;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<UserId>> Handle(GenerateUserCommand command, CancellationToken ct)
	{
		return await _userFactory
			.GenerateAndAddUserAsync(command.Name, command.Email, ct)
			.Then(user => user.Id)
			.TapAsync(_ => _unitOfWork.SaveChangesAsync(ct));
	}
}
