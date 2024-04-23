global using TeamGenerator = Bogus.Faker<TeamUp.TeamManagement.Domain.Aggregates.Teams.Team>;

using Bogus;

using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Contracts.Teams.ChangeNickname;
using TeamUp.TeamManagement.Contracts.Teams.CreateEventType;
using TeamUp.TeamManagement.Contracts.Teams.CreateTeam;
using TeamUp.TeamManagement.Contracts.Teams.SetTeamName;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.Tests.Common.Extensions;

using Xunit;

namespace TeamUp.Tests.Common.DataGenerators.TeamManagement;

public sealed class TeamGenerators : BaseGenerator
{
	internal const string TEAM_MEMBERS_FIELD = "_members";
	internal static readonly List<TeamMember> EmptyTeamMemberList = [];

	internal const string TEAM_EVENTTYPES_FIELD = "_eventTypes";
	internal static readonly List<EventType> EmptyEventTypeList = [];

	private static readonly PrivateBinder TeamBinder = new(TEAM_MEMBERS_FIELD, TEAM_EVENTTYPES_FIELD);
	private static readonly PrivateBinder TeamMemberBinder = new(
		nameof(TeamUp.TeamManagement.Domain.Aggregates.Teams.TeamMember.UserId).GetBackingField(),
		nameof(TeamUp.TeamManagement.Domain.Aggregates.Teams.TeamMember.TeamId).GetBackingField()
	);
	private static readonly PrivateBinder EventTypeBinder = new(
		nameof(TeamUp.TeamManagement.Domain.Aggregates.Teams.EventType.TeamId).GetBackingField()
	);

	public static readonly TeamGenerator Team = new TeamGenerator(binder: TeamBinder)
		.UsePrivateConstructor()
		.RuleFor(t => t.Id, f => TeamId.New())
		.RuleFor(t => t.Name, GenerateValidTeamName())
		.RuleFor(TEAM_EVENTTYPES_FIELD, f => EmptyEventTypeList)
		.RuleFor(TEAM_MEMBERS_FIELD, f => EmptyTeamMemberList);

	public static readonly Faker<TeamMember> TeamMember = new Faker<TeamMember>(binder: TeamMemberBinder)
		.UsePrivateConstructor()
		.RuleFor(tm => tm.Id, f => TeamMemberId.New());

	public static readonly Faker<EventType> EventType = new Faker<EventType>(binder: EventTypeBinder)
		.UsePrivateConstructor()
		.RuleFor(et => et.Id, f => EventTypeId.New())
		.RuleFor(et => et.Name, f => f.Random.Text(TeamConstants.EVENTTYPE_NAME_MIN_SIZE, TeamConstants.EVENTTYPE_NAME_MAX_SIZE))
		.RuleFor(et => et.Description, f => f.Random.Text(0, TeamConstants.EVENTTYPE_DESCRIPTION_MAX_SIZE));

	public static readonly Faker<UpsertEventTypeRequest> ValidUpsertEventTypeRequest = new Faker<UpsertEventTypeRequest>()
		.RuleFor(r => r.Name, f => f.Random.Text(TeamConstants.EVENTTYPE_NAME_MIN_SIZE, TeamConstants.EVENTTYPE_NAME_MAX_SIZE))
		.RuleFor(r => r.Description, f => f.Random.Text(0, TeamConstants.EVENTTYPE_DESCRIPTION_MAX_SIZE));

	public static string GenerateValidTeamName() => F.Random.Text(TeamConstants.TEAM_NAME_MIN_SIZE, TeamConstants.TEAM_NAME_MAX_SIZE);
	public static string GenerateValidNickname() => F.Random.Text(TeamConstants.NICKNAME_MIN_SIZE, TeamConstants.NICKNAME_MAX_SIZE);

	public sealed class InvalidUpsertEventTypeRequest : TheoryData<InvalidRequest<UpsertEventTypeRequest>>
	{
		public InvalidUpsertEventTypeRequest()
		{
			this.Add(x => x.Name, new UpsertEventTypeRequest
			{
				Name = "",
				Description = F.Random.AlphaNumeric(TeamConstants.EVENTTYPE_DESCRIPTION_MAX_SIZE - 1)
			});

			this.Add(x => x.Description, new UpsertEventTypeRequest
			{
				Name = F.Random.AlphaNumeric(TeamConstants.EVENTTYPE_NAME_MAX_SIZE - 1),
				Description = F.Random.AlphaNumeric(TeamConstants.EVENTTYPE_DESCRIPTION_MAX_SIZE + 1)
			});

			this.Add(x => x.Name, new UpsertEventTypeRequest
			{
				Name = F.Random.AlphaNumeric(TeamConstants.EVENTTYPE_NAME_MAX_SIZE + 1),
				Description = F.Random.AlphaNumeric(TeamConstants.EVENTTYPE_DESCRIPTION_MAX_SIZE - 1)
			});
		}
	}

	public sealed class InvalidCreateTeamRequest : TheoryData<InvalidRequest<CreateTeamRequest>>
	{
		public InvalidCreateTeamRequest()
		{
			this.Add(x => x.Name, new CreateTeamRequest
			{
				Name = "",
			});

			this.Add(x => x.Name, new CreateTeamRequest
			{
				Name = F.Random.AlphaNumeric(TeamConstants.TEAM_NAME_MIN_SIZE - 1),
			});

			this.Add(x => x.Name, new CreateTeamRequest
			{
				Name = F.Random.AlphaNumeric(TeamConstants.TEAM_NAME_MAX_SIZE + 1),
			});
		}
	}

	public sealed class InvalidChangeNicknameRequest : TheoryData<InvalidRequest<ChangeNicknameRequest>>
	{
		public InvalidChangeNicknameRequest()
		{
			this.Add(x => x.Nickname, new ChangeNicknameRequest
			{
				Nickname = "",
			});

			this.Add(x => x.Nickname, new ChangeNicknameRequest
			{
				Nickname = F.Random.AlphaNumeric(TeamConstants.NICKNAME_MIN_SIZE - 1),
			});

			this.Add(x => x.Nickname, new ChangeNicknameRequest
			{
				Nickname = F.Random.AlphaNumeric(TeamConstants.NICKNAME_MAX_SIZE + 1),
			});
		}
	}

	public sealed class InvalidUpdateTeamNameRequest : TheoryData<InvalidRequest<UpdateTeamNameRequest>>
	{
		public InvalidUpdateTeamNameRequest()
		{
			this.Add(x => x.Name, new UpdateTeamNameRequest
			{
				Name = "",
			});

			this.Add(x => x.Name, new UpdateTeamNameRequest
			{
				Name = F.Random.AlphaNumeric(TeamConstants.TEAM_NAME_MIN_SIZE - 1),
			});

			this.Add(x => x.Name, new UpdateTeamNameRequest
			{
				Name = F.Random.AlphaNumeric(TeamConstants.TEAM_NAME_MAX_SIZE + 1),
			});
		}
	}
}
