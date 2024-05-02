using Microsoft.Extensions.Logging;

using RailwayResult;
using RailwayResult.Errors;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts.Errors;
using TeamUp.Common.Domain;
using TeamUp.Notifications.Contracts;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations.IntegrationEvents;
using TeamUp.TeamManagement.Domain.Aggregates.Users;

namespace TeamUp.TeamManagement.Application.Invitations;

internal sealed class CreateInvitationRequestCreatedEventHandler : IIntegrationEventHandler<CreateInvitationRequestCreatedIntegrationEvent>
{
	private readonly IUserRepository _userRepository;
	private readonly ILogger<CreateInvitationRequestCreatedEventHandler> _logger;
	private readonly InvitationFactory _invitationFactory;
	private readonly IUnitOfWork<TeamManagementModuleId> _unitOfWork;
	private readonly IIntegrationEventPublisher<TeamManagementModuleId> _publisher;

	public CreateInvitationRequestCreatedEventHandler(IUserRepository userRepository, ILogger<CreateInvitationRequestCreatedEventHandler> logger, InvitationFactory invitationFactory, IUnitOfWork<TeamManagementModuleId> unitOfWork, IIntegrationEventPublisher<TeamManagementModuleId> publisher)
	{
		_userRepository = userRepository;
		_logger = logger;
		_invitationFactory = invitationFactory;
		_unitOfWork = unitOfWork;
		_publisher = publisher;
	}

	public async Task<Result> Handle(CreateInvitationRequestCreatedIntegrationEvent integrationEvent, CancellationToken ct)
	{
		var user = await _userRepository.GetUserByEmailAsync(integrationEvent.Email, ct);
		if (user is null)
		{
			_logger.LogWarning("User {userEmail} is not yet created for inviting to team {teamId}.", integrationEvent.Email, integrationEvent.TeamId);
			return new EventualConsistencyError("TeamManagement.Users.NotFound", "User not found when inviting.");
		}

		var result = await _invitationFactory
			.CreateAndAddInvitationAsync(user.Id, integrationEvent.TeamId, ct)
			.Tap(invitation =>
			{
				var message = new EmailCreatedIntegrationEvent
				{
					Email = user.Email,
					Subject = "You were invited to team!",
					Message = $"Accept invitation link for {invitation.Id}."
				};

				_publisher.Publish(message);
			})
			.ThenAsync(_ => _unitOfWork.SaveChangesAsync(ct));

		if (result.IsFailure)
		{
			if (result.Error is ConflictError error)
			{
				_logger.LogWarning("Invitation of user {userEmail} to team {teamId} probably already exist. {error}", integrationEvent.Email, integrationEvent.TeamId, error);
				return Result.Success;
			}
		}

		return result;
	}
}
