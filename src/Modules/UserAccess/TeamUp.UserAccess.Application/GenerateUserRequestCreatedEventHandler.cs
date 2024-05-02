using TeamUp.Common.Application;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.CreateUser;
using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Application;

internal sealed class GenerateUserRequestCreatedEventHandler : IIntegrationEventHandler<GenerateUserRequestCreatedIntegrationEvent>
{
	private readonly UserFactory _userFactory;
	private readonly IUnitOfWork<UserAccessModuleId> _unitOfWork;

	public GenerateUserRequestCreatedEventHandler(
		UserFactory userFactory,
		IUnitOfWork<UserAccessModuleId> unitOfWork)
	{
		_userFactory = userFactory;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(GenerateUserRequestCreatedIntegrationEvent command, CancellationToken ct)
	{
		return await _userFactory
			.GenerateAndAddUserAsync(command.Name, command.Email, ct)
			.ThenAsync(_ => _unitOfWork.SaveChangesAsync(ct));
	}
}
