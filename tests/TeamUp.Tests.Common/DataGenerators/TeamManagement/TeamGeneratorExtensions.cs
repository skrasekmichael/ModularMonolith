using FluentAssertions;

using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Users;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.Extensions;

namespace TeamUp.Tests.Common.DataGenerators.TeamManagement;

public static class TeamGeneratorExtensions
{
	private static TeamRole GetOwner(this User user)
	{
		user.IncreaseNumberOfOwningTeams();
		return TeamRole.Owner;
	}

	public static TeamGenerator WithMembers(this TeamGenerator teamGenerator, User user, TeamRole role, List<User> members)
	{
		var hasOwner = role == TeamRole.Owner || members.Count >= 1;
		hasOwner.Should().Be(true, "team has to have exactly 1 owner");

		return role switch
		{
			TeamRole.Owner => teamGenerator.GetTeamGeneratorWithMembers(members, (user, TeamRole.Owner)),
			_ => teamGenerator.GetTeamGeneratorWithMembers(members[1..], (members.First(), TeamRole.Owner), (user, role))
		};
	}

	public static TeamGenerator WithMembers(this TeamGenerator teamGenerator, User user1, TeamRole role1, User user2, TeamRole role2, List<User> members)
	{
		var hasOwner = role1 == TeamRole.Owner || role2 == TeamRole.Owner || members.Count >= 1;
		hasOwner.Should().Be(true, "team has to have exactly 1 owner");

		return (role1, role2) switch
		{
			(TeamRole.Owner, _) or (_, TeamRole.Owner) => teamGenerator.GetTeamGeneratorWithMembers(members, (user1, role1), (user2, role2)),
			_ => teamGenerator.GetTeamGeneratorWithMembers(members[1..], (members.First(), TeamRole.Owner), (user1, role1), (user2, role2))
		};
	}

	public static TeamGenerator WithMembers(this TeamGenerator teamGenerator, User owner, List<User> members, params (User User, TeamRole Role)[] userMembers)
	{
		var userMembersWithOwner = new (User User, TeamRole Role)[userMembers.Length + 1];
		userMembersWithOwner[0] = (owner, TeamRole.Owner);
		userMembers.CopyTo(userMembersWithOwner, 1);
		return teamGenerator.GetTeamGeneratorWithMembers(members, userMembersWithOwner);
	}

	public static TeamGenerator WithRandomMembers(this TeamGenerator teamGenerator, int count, IEnumerable<User> pot, int lastXNonOwners = 0)
	{
		count.Should().BeGreaterThan(0);
		pot.Should().HaveCountGreaterThanOrEqualTo(1);

		return teamGenerator
			.RuleFor(t => t.NumberOfMembers, count)
			.RuleFor(TeamGenerators.TEAM_MEMBERS_FIELD, (f, t) => f
				.Make(() => new List<User>(pot), count, (list, index) => f.PopRandom(list, index == 1 ? lastXNonOwners : 0)
					.Map(user => TeamGenerators.TeamMember
						.RuleForBackingField(tm => tm.TeamId, t.Id)
						.RuleForBackingField(tm => tm.UserId, user.Id)
						.RuleFor(tm => tm.Nickname, user.Name)
						.RuleFor(tm => tm.Role, index == 1 ? user.GetOwner() : TeamRole.Member)
						.Generate()))
				.ToList());
	}

	public static TeamGenerator WithMembers(this TeamGenerator teamGenerator, List<User> members, params (User User, TeamRole Role)[] userMembers)
	{
		userMembers.Where(x => x.Role == TeamRole.Owner).Should().ContainSingle("team has to have exactly 1 owner");
		members.Should().NotContain(userMembers.Select(x => x.User));
		return teamGenerator.GetTeamGeneratorWithMembers(members, userMembers);
	}

	private static TeamGenerator GetTeamGeneratorWithMembers(this TeamGenerator teamGenerator, List<User> members, params (User User, TeamRole Role)[] userMembers)
	{
		return teamGenerator
			.RuleFor(t => t.NumberOfMembers, members.Count + userMembers.Length)
			.RuleFor(TeamGenerators.TEAM_MEMBERS_FIELD, (f, t) => userMembers
				.Select(um => TeamGenerators.TeamMember
					.RuleForBackingField(tm => tm.TeamId, t.Id)
					.RuleForBackingField(tm => tm.UserId, um.User.Id)
					.RuleFor(tm => tm.Nickname, um.User.Name)
					.RuleFor(tm => tm.Role, um.Role == TeamRole.Owner ? um.User.GetOwner() : um.Role)
					.Generate())
				.Concat(members
					.Select(member => TeamGenerators.TeamMember
						.RuleForBackingField(tm => tm.TeamId, t.Id)
						.RuleForBackingField(tm => tm.UserId, member.Id)
						.RuleFor(tm => tm.Nickname, member.Name)
						.RuleFor(tm => tm.Role, TeamRole.Member)
						.Generate()))
				.ToList()
			);
	}

	public static TeamGenerator WithOneOwner(this TeamGenerator teamGenerator, User owner)
	{
		return teamGenerator
			.RuleFor(t => t.NumberOfMembers, 1)
			.RuleFor(TeamGenerators.TEAM_MEMBERS_FIELD, (f, t) => new List<TeamMember>()
			{
				TeamGenerators.TeamMember
					.RuleForBackingField(tm => tm.TeamId, t.Id)
					.RuleForBackingField(tm => tm.UserId, owner.Id)
					.RuleFor(tm => tm.Nickname, owner.Name)
					.RuleFor(tm => tm.Role, owner.GetOwner())
					.Generate()
			});
	}

	public static TeamGenerator WithEventTypes(this TeamGenerator teamGenerator, int count)
	{
		return teamGenerator
			.RuleFor(TeamGenerators.TEAM_EVENTTYPES_FIELD, (f, t) => TeamGenerators.EventType
				.RuleForBackingField(et => et.TeamId, t.Id)
				.Generate(count));
	}
}
