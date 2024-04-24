using RailwayResult;

using TeamUp.Common.Application;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Contracts.Invitations.InviteUser;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;

namespace TeamUp.TeamManagement.Application.Invitations;

internal sealed class InviteUserCommandHandler : ICommandHandler<InviteUserCommand>
{
	private readonly IInvitationDomainService _invitationDomainService;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;

	public InviteUserCommandHandler(IInvitationDomainService invitationDomainService, IUnitOfWork<TeamManagementModuleId> unitOfWork)
	{
		_invitationDomainService = invitationDomainService;
		_unitOfWork = unitOfWork;
	}

	public Task<Result> Handle(InviteUserCommand command, CancellationToken ct)
	{
		return _invitationDomainService
			.InviteUserAsync(command.InitiatorId, command.TeamId, command.Email, ct)
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
