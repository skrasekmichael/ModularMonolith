using RailwayResult.Errors;

using TeamUp.TeamManagement.Contracts.Teams;

namespace TeamUp.TeamManagement.Domain.Aggregates.Teams;

public static class TeamErrors
{
	public static readonly ValidationError TeamNameMinSize = new("TeamManagement.Teams.Validation.NameMinSize", $"Name must be atleast {TeamConstants.TEAM_NAME_MIN_SIZE} characters long.");
	public static readonly ValidationError TeamNameMaxSize = new("TeamManagement.Teams.Validation.NameMaxSize", $"Name must be shorter than {TeamConstants.TEAM_NAME_MAX_SIZE} characters.");
	public static readonly ValidationError NicknameMinSize = new("TeamManagement.Teams.Validation.Members.NicknameMinSize", $"Nickname must be atleast {TeamConstants.NICKNAME_MIN_SIZE} characters long.");
	public static readonly ValidationError NicknameMaxSize = new("TeamManagement.Teams.Validation.Members.NicknameMaxSize", $"Nickname must be shorter than {TeamConstants.NICKNAME_MAX_SIZE} characters.");
	public static readonly ValidationError EventTypeNameMinSize = new("TeamManagement.Teams.Validation.EventTypes.NameMinSize", $"EventType's name must be atleast {TeamConstants.EVENTTYPE_NAME_MIN_SIZE} characters long.");
	public static readonly ValidationError EventTypeNameMaxSize = new("TeamManagement.Teams.Validation.EventTypes.NameMaxSize", $"EventType's name must be shorter than {TeamConstants.EVENTTYPE_NAME_MAX_SIZE} characters.");
	public static readonly ValidationError EventTypeDescriptionMaxSize = new("TeamManagement.Teams.Validation.EventTypes.DescriptionMaxSize", $"EventType's description must be shorter than {TeamConstants.EVENTTYPE_NAME_MAX_SIZE} characters.");

	public static readonly AuthorizationError NotMemberOfTeam = new("TeamManagement.Teams.Authorization.NotMember", "Not member of the team.");
	public static readonly AuthorizationError UnauthorizedToChangeTeamName = new("TeamManagement.Teams.Authorization.ChangeTeam", "Not allowed to change team name.");
	public static readonly AuthorizationError UnauthorizedToChangeTeamOwnership = new("TeamManagement.Teams.Authorization.ChangeOwner", "Not allowed to change ownership.");
	public static readonly AuthorizationError UnauthorizedToUpdateTeamRoles = new("TeamManagement.Teams.Authorization.UpdateRole", "Not allowed to update team roles.");
	public static readonly AuthorizationError UnauthorizedToRemoveTeamMembers = new("TeamManagement.Teams.Authorization.RemoveMember", "Not allowed to remove team members.");
	public static readonly AuthorizationError UnauthorizedToCreateEvents = new("TeamManagement.Teams.Authorization.CreateEvent", "Not allowed to create events.");
	public static readonly AuthorizationError UnauthorizedToInviteTeamMembers = new("TeamManagement.Teams.Authorization.InviteUser", "Not allowed to invite team members.");
	public static readonly AuthorizationError UnauthorizedToCancelInvitations = new("TeamManagement.Teams.Authorization.CancelInvitation", "Not allowed to cancel invitations.");
	public static readonly AuthorizationError UnauthorizedToReadInvitationList = new("TeamManagement.Teams.Authorization.ReadInvitations", "Not allowed to read invitation list.");
	public static readonly AuthorizationError UnauthorizedToDeleteTeam = new("TeamManagement.Teams.Authorization.DeleteTeam", "Not allowed to delete team.");
	public static readonly AuthorizationError UnauthorizedToCreateEventTypes = new("TeamManagement.Teams.Authorization.CreateEventType", "Not allowed to create event types.");
	public static readonly AuthorizationError UnauthorizedToDeleteEvents = new("TeamManagement.Teams.Authorization.DeleteEvent", "Not allowed to delete events.");

	public static readonly NotFoundError TeamNotFound = new("TeamManagement.Teams.NotFound", "Team not found.");
	public static readonly NotFoundError MemberNotFound = new("TeamManagement.Teams.NotFound.Members", "Member not found.");
	public static readonly NotFoundError EventTypeNotFound = new("TeamManagement.Teams.NotFound.EventTypes", "Event type not found.");

	public static readonly DomainError CannotChangeTeamOwnersRole = new("TeamManagement.Teams.Domain.ChangeOwnersRole", "Cannot change role of the team owner.");
	public static readonly DomainError CannotHaveMultipleTeamOwners = new("TeamManagement.Teams.Domain.MultipleOwners", "Cannot have multiple team owners.");
	public static readonly DomainError CannotRemoveTeamOwner = new("TeamManagement.Teams.Domain.RemoveOwner", "Cannot remove owner of the team.");
	public static readonly DomainError CannotInviteUserThatIsTeamMember = new("TeamManagement.Teams.Domain.InviteMember", "Cannot invite user that is already member of the team.");
	public static readonly DomainError CannotOwnSoManyTeams = new("TeamManagement.Teams.Domain.OwnsToManyTeams", $"Cannot own more than {TeamConstants.MAX_NUMBER_OF_OWNED_TEAMS} teams.");
	public static readonly DomainError MaximumCapacityReached = new("TeamManagement.Teams.Domain.MaximumCapacity", $"Team has reached maximum capacity of {TeamConstants.MAX_TEAM_CAPACITY} members.");
}
