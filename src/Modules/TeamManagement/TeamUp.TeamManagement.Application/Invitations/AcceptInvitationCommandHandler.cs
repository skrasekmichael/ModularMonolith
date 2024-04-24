using RailwayResult;

using TeamUp.Common.Application;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Contracts.Invitations.AcceptInvitation;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;

namespace TeamUp.TeamManagement.Application.Invitations;

internal sealed class AcceptInvitationCommandHandler : ICommandHandler<AcceptInvitationCommand>
{
	private readonly IInvitationDomainService _invitationDomainService;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;

	public AcceptInvitationCommandHandler(IInvitationDomainService invitationDomainService, IUnitOfWork<TeamManagementModuleId> unitOfWork)
	{
		_invitationDomainService = invitationDomainService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(AcceptInvitationCommand command, CancellationToken ct)
	{
		return await _invitationDomainService
			.AcceptInvitationAsync(command.InitiatorId, command.InvitationId, ct)
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
