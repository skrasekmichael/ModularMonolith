using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Infrastructure.Services;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Teams;

public sealed class ChangeOwnershipTests(AppFixture app) : TeamTests(app)
{
	public static string GetUrl(TeamId teamId) => GetUrl(teamId.Value);
	public static string GetUrl(Guid teamId) => $"/api/v1/teams/{teamId}/owner";

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	public async Task ChangeOwnership_ToAdminOrLower_AsOwner_Should_ChangeTeamOwnerInDatabase(TeamRole teamRole)
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var targetUserUA = UserGenerators.User.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(18);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(owner, members, (targetUser, teamRole)).Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([ownerUA, targetUserUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([owner, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(ownerUA);

		var targetMemberId = team.Members.First(member => member.UserId == targetUser.Id).Id;

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberId.Value);

		//act
		response.Should().Be200Ok();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			var teamMembers = await dbContext
				.Set<TeamMember>()
				.Where(member => member.TeamId == team.Id)
				.ToListAsync();

			var originalOwner = teamMembers.SingleOrDefault(member => member.UserId == owner.Id);
			originalOwner.ShouldNotBeNull();
			originalOwner.Role.Should().Be(TeamRole.Admin);

			var newOwner = teamMembers.SingleOrDefault(member => member.UserId == targetUser.Id);
			newOwner.ShouldNotBeNull();
			newOwner.Role.Should().Be(TeamRole.Owner);

			teamMembers
				.Except<TeamMember>([originalOwner, newOwner])
				.Should()
				.Contain(member => TeamContainsMemberWithSameRole(team, member));
		});
	}

	[Theory]
	[InlineData(TeamRole.Member, TeamRole.Member)]
	[InlineData(TeamRole.Member, TeamRole.Coordinator)]
	[InlineData(TeamRole.Member, TeamRole.Admin)]
	[InlineData(TeamRole.Coordinator, TeamRole.Member)]
	[InlineData(TeamRole.Coordinator, TeamRole.Coordinator)]
	[InlineData(TeamRole.Coordinator, TeamRole.Admin)]
	[InlineData(TeamRole.Admin, TeamRole.Member)]
	[InlineData(TeamRole.Admin, TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin, TeamRole.Admin)]
	public async Task ChangeOwnership_ToAdminOrLower_AsAdminOrLower_Should_ResultInForbidden(TeamRole initiatorTeamRole, TeamRole targetTeamRole)
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var targetUserUA = UserGenerators.User.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(18);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(owner, members, (targetUser, targetTeamRole), (initiatorUser, initiatorTeamRole)).Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([ownerUA, initiatorUserUA, targetUserUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var targetMemberId = team.Members.First(member => member.UserId == initiatorUser.Id).Id;

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberId.Value);

		//act
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToChangeTeamOwnership);
	}

	[Fact]
	public async Task ChangeOwnership_ToUnExistingMember_AsOwner_Should_ResultInNotFound()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.Add(ownerUA);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.Add(owner);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(ownerUA);

		var targetMemberId = Guid.NewGuid();

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberId);

		//act
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.MemberNotFound);
	}

	[Fact]
	public async Task ChangeOwnership_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var targetUserUA = UserGenerators.User.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(18);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(owner, members, (targetUser, TeamRole.Member)).Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([ownerUA, initiatorUserUA, targetUserUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var targetMemberId = team.Members.First(member => member.UserId == targetUser.Id).Id.Value;

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberId);

		//act
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task ChangeOwnership_OfUnExistingTeam_Should_ResultInNotFound()
	{
		//arrange
		var userUA = UserGenerators.User.Generate();
		var user = userUA.ToUserTM();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.Add(userUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(userUA);

		var teamId = Guid.NewGuid();
		var targetMemberId = Guid.NewGuid();

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(teamId), targetMemberId);

		//act
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task ChangeOwnership_AsOwner_WhenConcurrentUpdateCompletes_Should_ResultInConflict()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var targetUserAUA = UserGenerators.User.Generate();
		var targetUserA = targetUserAUA.ToUserTM();

		var targetUserBUA = UserGenerators.User.Generate();
		var targetUserB = targetUserBUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(17);
		var members = membersUA.ToUsersTM();

		var targetUserUA = UserGenerators.User.Generate();

		var team = TeamGenerators.Team
			.WithMembers(owner, members, (targetUserA, TeamRole.Member), (targetUserB, TeamRole.Member))
			.Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([ownerUA, targetUserAUA, targetUserBUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([owner, targetUserA, targetUserB]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(ownerUA);

		var targetMemberAId = team.Members.First(member => member.UserId == targetUserA.Id).Id;
		var targetMemberBId = team.Members.First(member => member.UserId == targetUserB.Id).Id;

		//assert
		var (responseA, responseB) = await RunConcurrentRequestsAsync<TeamManagementModuleId>(
			() => Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberAId.Value),
			() => Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberBId.Value)
		);

		//act
		responseA.Should().Be200Ok();
		responseB.Should().Be409Conflict();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			var teamMembers = await dbContext
				.Set<TeamMember>()
				.Where(member => member.TeamId == team.Id)
				.ToListAsync();

			var originalOwner = teamMembers.Single(member => member.UserId == owner.Id);
			originalOwner.Role.Should().Be(TeamRole.Admin);

			var newOwner = teamMembers.Single(member => member.UserId == targetUserA.Id);
			newOwner.Role.Should().Be(TeamRole.Owner);

			var concurrentTarget = teamMembers.Single(member => member.UserId == targetUserB.Id);
			concurrentTarget.Role.Should().Be(TeamRole.Member);

			teamMembers
				.Except<TeamMember>([originalOwner, newOwner])
				.Should()
				.Contain(member => TeamContainsMemberWithSameRole(team, member));
		});

		var problemDetails = await responseB.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UnitOfWork<TeamManagementDbContext, TeamManagementModuleId>.ConcurrencyError);
	}
}
