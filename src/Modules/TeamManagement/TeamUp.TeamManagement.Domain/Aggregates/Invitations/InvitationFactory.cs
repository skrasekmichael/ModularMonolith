using RailwayResult;

using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Domain.Aggregates.Invitations;

internal sealed class InvitationFactory
{
	private readonly IInvitationRepository _invitationRepository;
	private readonly IDateTimeProvider _dateTimeProvider;

	public InvitationFactory(IInvitationRepository invitationRepository, IDateTimeProvider dateTimeProvider)
	{
		_invitationRepository = invitationRepository;
		_dateTimeProvider = dateTimeProvider;
	}

	public async Task<Result<Invitation>> CreateInvitationAsync(UserId userId, TeamId teamId, CancellationToken ct)
	{
		if (await _invitationRepository.ExistsInvitationForUserToTeamAsync(userId, teamId, ct))
		{
			return InvitationErrors.UserIsAlreadyInvited;
		}

		return new Invitation(InvitationId.New(), userId, teamId, _dateTimeProvider.UtcNow);
	}
}
