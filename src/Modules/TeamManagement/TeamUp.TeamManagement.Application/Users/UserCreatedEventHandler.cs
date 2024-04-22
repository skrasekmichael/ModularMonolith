using RailwayResult;

using TeamUp.Common.Application;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Domain.Aggregates.Users;
using TeamUp.UserAccess.Contracts.CreateUser;

namespace TeamUp.TeamManagement.Application.Users;

internal sealed class UserCreatedEventHandler : IIntegrationEventHandler<UserCreatedIntegrationEvent>
{
	private readonly IUserRepository _userRepository;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;

	public UserCreatedEventHandler(IUserRepository userRepository, IUnitOfWork<TeamManagementModuleId> unitOfWork)
	{
		_userRepository = userRepository;
		_unitOfWork = unitOfWork;
	}

	public Task<Result> Handle(UserCreatedIntegrationEvent integrationEvent, CancellationToken ct)
	{
		var user = new User(integrationEvent.UserId, integrationEvent.Name, integrationEvent.Email);
		_userRepository.AddUser(user);
		return _unitOfWork.SaveChangesAsync(ct);
	}
}
