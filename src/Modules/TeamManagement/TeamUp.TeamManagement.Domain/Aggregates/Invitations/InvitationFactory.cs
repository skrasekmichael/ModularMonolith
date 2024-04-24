using RailwayResult;

using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Domain.Aggregates.Invitations;

public sealed class InvitationFactory
{
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly IInvitationRepository _invitationRepository;

	public InvitationFactory(IInvitationRepository invitationRepository, IDateTimeProvider dateTimeProvider)
	{
		_invitationRepository = invitationRepository;
		_dateTimeProvider = dateTimeProvider;
	}

	public async Task<Result<Invitation>> CreateAndAddInvitationAsync(UserId userId, TeamId teamId, CancellationToken ct)
	{
		if (await _invitationRepository.ExistsInvitationForUserToTeamAsync(userId, teamId, ct))
		{
			return InvitationErrors.UserIsAlreadyInvited;
		}

		var invitation = new Invitation(InvitationId.New(), userId, teamId, _dateTimeProvider.UtcNow);
		_invitationRepository.AddInvitation(invitation);

		return invitation;
	}
}
