using RailwayResult;

using TeamUp.Common.Application;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Contracts.Invitations.RemoveInvitation;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;

namespace TeamUp.TeamManagement.Application.Invitations;

internal sealed class RemoveInvitationCommandHandler : ICommandHandler<RemoveInvitationCommand>
{
	private readonly IInvitationDomainService _invitationDomainService;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;

	public RemoveInvitationCommandHandler(IInvitationDomainService invitationDomainService, IUnitOfWork<TeamManagementModuleId> unitOfWork)
	{
		_invitationDomainService = invitationDomainService;
		_unitOfWork = unitOfWork;
	}

	public Task<Result> Handle(RemoveInvitationCommand command, CancellationToken ct)
	{
		return _invitationDomainService
			.RemoveInvitationAsync(command.InitiatorId, command.InvitationId, ct)
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
