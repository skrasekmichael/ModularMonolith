using RailwayResult;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Contracts;
using TeamUp.Common.Domain;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Teams.DomainEvents;
using TeamUp.TeamManagement.Domain.Aggregates.Users;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Domain.Aggregates.Teams;

public sealed class Team : AggregateRoot<Team, TeamId>
{
	private readonly List<EventType> _eventTypes = [];
	private readonly List<TeamMember> _members = [];

	public string Name { get; private set; }
	public int NumberOfMembers { get; private set; }

	public IReadOnlyList<EventType> EventTypes => _eventTypes.AsReadOnly();
	public IReadOnlyList<TeamMember> Members => _members.AsReadOnly();

#pragma warning disable CS8618 // EF Core constructor
	private Team() : base() { }
#pragma warning restore CS8618

	private Team(TeamId id, string name) : base(id)
	{
		Name = name;
	}

	public static Result<Team> Create(string name, User owner, IDateTimeProvider dateTimeProvider)
	{
		return name
			.Ensure(TeamRules.TeamNameMinSize, TeamRules.TeamNameMaxSize)
			.Then(name => new Team(TeamId.New(), name))
			.Tap(team => team.AddTeamMember(owner, dateTimeProvider, TeamRole.Owner))
			.Tap(_ => owner.IncreaseNumberOfOwningTeams());
	}

	public Result Delete(UserId initiatorId)
	{
		return GetTeamMemberByUserId(initiatorId)
			.Ensure(TeamRules.MemberIsOwner, TeamErrors.UnauthorizedToDeleteTeam)
			.Tap(_ => AddDomainEvent(new TeamDeletedDomainEvent(this)))
			.ToResult();
	}

	public void DecreaseNumberOfMembers() => NumberOfMembers--;

	public Result<EventTypeId> CreateEventType(UserId initiatorId, string name, string description)
	{
		return GetTeamMemberByUserId(initiatorId)
			.Ensure(TeamRules.MemberCanCreateEventTypes)
			.Then(_ => EventType.Create(name, description, this))
			.Tap(_eventTypes.Add)
			.Then(eventType => eventType.Id);
	}

	internal void AddTeamMember(User user, IDateTimeProvider dateTimeProvider, TeamRole role = TeamRole.Member)
	{
		if (_members.Find(member => member.UserId == user.Id) is not null)
			return;

		_members.Add(new TeamMember(
			TeamMemberId.New(),
			user.Id,
			this,
			user.Name,
			role,
			dateTimeProvider.DateTimeOffsetUtcNow
		));
		NumberOfMembers += 1;
	}

	public Result RemoveTeamMember(UserId initiatorId, TeamMemberId teamMemberId)
	{
		return GetTeamMember(teamMemberId)
			.Ensure(TeamRules.MemberIsNotTeamOwner, TeamErrors.CannotRemoveTeamOwner)
			.And(() => GetTeamMemberByUserId(initiatorId))
			.Ensure(TeamRules.MemberCanBeRemovedByInitiator)
			.Tap((member, _) => _members.Remove(member))
			.Tap(_ => NumberOfMembers--)
			.ToResult();
	}

	public Result ChangeNickname(UserId initiatorId, string newNickname)
	{
		return newNickname
			.Ensure(TeamRules.NicknameMinSize, TeamRules.NicknameMaxSize)
			.Then(_ => GetTeamMemberByUserId(initiatorId))
			.Tap(initiator => initiator.UpdateNickname(newNickname))
			.ToResult();
	}

	public Result SetMemberRole(UserId initiatorId, TeamMemberId memberId, TeamRole newRole)
	{
		return newRole
			.Ensure(TeamRules.RoleIsNotOwner, TeamErrors.CannotHaveMultipleTeamOwners)
			.Then(_ => GetTeamMemberByUserId(initiatorId))
			.Ensure(TeamRules.MemberCanUpdateTeamRoles)
			.Then(_ => GetTeamMember(memberId))
			.Ensure(TeamRules.MemberIsNotTeamOwner, TeamErrors.CannotChangeTeamOwnersRole)
			.Tap(teamMember => teamMember.UpdateRole(newRole))
			.ToResult();
	}

	public Result ChangeOwnership(UserId initiatorId, TeamMemberId memberId)
	{
		return GetTeamMemberByUserId(initiatorId)
			.Ensure(TeamRules.MemberCanChangeOwnership)
			.And(() => GetTeamMember(memberId))
			.Tap((initiator, member) =>
			{
				initiator.UpdateRole(TeamRole.Admin);
				member.UpdateRole(TeamRole.Owner);
				AddDomainEvent(new TeamOwnershipChangedDomainEvent(initiator, member));
			})
			.ToResult();
	}

	public Result ChangeTeamName(UserId initiatorId, string newName)
	{
		return newName
			.Ensure(TeamRules.TeamNameMinSize, TeamRules.TeamNameMaxSize)
			.Then(_ => GetTeamMemberByUserId(initiatorId))
			.Ensure(TeamRules.MemberCanChangeTeamName)
			.Then(_ => Name = newName)
			.ToResult();
	}

	public Result<TeamMember> GetTeamMemberByUserId(UserId userId)
	{
		var teamMember = _members.Find(member => member.UserId == userId);
		return teamMember.EnsureNotNull(TeamErrors.NotMemberOfTeam);
	}

	private Result<TeamMember> GetTeamMember(TeamMemberId memberId)
	{
		var teamMember = _members.Find(member => member.Id == memberId);
		return teamMember.EnsureNotNull(TeamErrors.MemberNotFound);
	}

	public TeamMember? GetHighestNonOwnerTeamMember()
	{
		return _members.OrderBy(member => member.Role)
			.SkipLast(1)
			.LastOrDefault();
	}
}
