using MassTransit;

using RailwayResult;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Contracts;
using TeamUp.Common.Domain;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations.IntegrationEvents;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Users;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.CreateUser;

namespace TeamUp.TeamManagement.Domain.Aggregates.Invitations;

internal sealed class InvitationDomainService : IInvitationDomainService
{
	private readonly IUserRepository _userRepository;
	private readonly ITeamRepository _teamRepository;
	private readonly IInvitationRepository _invitationRepository;
	private readonly IIntegrationEventPublisher<TeamManagementModuleId> _publisher;
	private readonly IDateTimeProvider _dateTimeProvider;

	public InvitationDomainService(
		IUserRepository userRepository,
		ITeamRepository teamRepository,
		IInvitationRepository invitationRepository,
		IIntegrationEventPublisher<TeamManagementModuleId> publisher,
		IDateTimeProvider dateTimeProvider)
	{
		_userRepository = userRepository;
		_teamRepository = teamRepository;
		_invitationRepository = invitationRepository;
		_publisher = publisher;
		_dateTimeProvider = dateTimeProvider;
	}

	public async Task<Result> AcceptInvitationAsync(UserId initiatorId, InvitationId invitationId, CancellationToken ct = default)
	{
		var invitation = await _invitationRepository.GetInvitationByIdAsync(invitationId, ct);
		return await invitation
			.EnsureNotNull(InvitationErrors.InvitationNotFound)
			.Ensure(invitation => invitation.RecipientId == initiatorId, InvitationErrors.UnauthorizedToAcceptInvitation)
			.Ensure(invitation => !invitation.HasExpired(_dateTimeProvider.UtcNow), InvitationErrors.InvitationExpired)
			.AndAsync(invitation => _teamRepository.GetTeamByIdAsync(invitation.TeamId, ct))
			.EnsureSecondNotNull(TeamErrors.TeamNotFound)
			.ThenAsync(async (invitation, team) =>
			{
				return await team
					.Ensure(TeamRules.TeamHasNotReachedCapacity)
					.ThenAsync(_ => _userRepository.GetUserByIdAsync(invitation.RecipientId))
					.EnsureNotNull(UserErrors.UserNotFound)
					.Tap(user =>
					{
						team.AddTeamMember(user, _dateTimeProvider);
						_invitationRepository.RemoveInvitation(invitation);
					});
			})
			.ToResultAsync();
	}


	public async Task<Result> InviteUserAsync(UserId initiatorId, TeamId teamId, string email, CancellationToken ct = default)
	{
		var team = await _teamRepository.GetTeamByIdAsync(teamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Ensure(TeamRules.TeamHasNotReachedCapacity)
			.And(team => team.GetTeamMemberByUserId(initiatorId))
			.Ensure((_, member) => member.Role.CanInviteTeamMembers(), TeamErrors.UnauthorizedToInviteTeamMembers)
			.Then((team, _) => team)
			.AndAsync(team => _userRepository.GetUserByEmailAsync(email, ct))
			.Ensure(TeamRules.InvitedUserIsNotTeamMember)
			.ThenAsync<Team, User?, (Team, string)>(async (team, user) =>
			{
				//generate user if user doesn't exist
				if (user is null)
				{
					var message = new GenerateUserRequestCreatedIntegrationEvent
					{
						Email = email,
						Name = email
					};

					_publisher.Publish(message);
					return (team, email);
				}

				//check whether user is already invited to the same team
				var conflictingInvitationExists = await _invitationRepository.ExistsInvitationForUserToTeamAsync(user.Id, teamId, ct);
				if (conflictingInvitationExists)
				{
					return InvitationErrors.UserIsAlreadyInvited;
				}

				return (team, user.Email);
			})
			.Tap((team, email) =>
			{
				var message = new CreateInvitationRequestCreatedIntegrationEvent
				{
					TeamId = team.Id,
					Email = email
				};

				_publisher.Publish(message);
			})
			.ToResultAsync();
	}

	public async Task<Result> RemoveInvitationAsync(UserId initiatorId, InvitationId invitationId, CancellationToken ct = default)
	{
		var invitation = await _invitationRepository.GetInvitationByIdAsync(invitationId, ct);
		return await invitation
			.EnsureNotNull(InvitationErrors.InvitationNotFound)
			.ThenAsync(async invitation =>
			{
				if (invitation.RecipientId == initiatorId)
					return invitation;

				var team = await _teamRepository.GetTeamByIdAsync(invitation.TeamId, ct);
				return team
					.EnsureNotNull(TeamErrors.NotMemberOfTeam)
					.Then(team => team.GetTeamMemberByUserId(initiatorId))
					.Ensure(member => member.Role.CanInviteTeamMembers(), TeamErrors.UnauthorizedToCancelInvitations)
					.Then(_ => invitation);
			})
			.Tap(_invitationRepository.RemoveInvitation)
			.ToResultAsync();
	}
}
