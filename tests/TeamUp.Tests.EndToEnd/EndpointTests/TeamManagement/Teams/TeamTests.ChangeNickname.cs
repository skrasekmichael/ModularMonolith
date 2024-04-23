using Microsoft.EntityFrameworkCore;

using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Contracts.Teams.ChangeNickname;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Teams;

public sealed class ChangeNicknameTests(AppFixture app) : TeamTests(app)
{
	public static string GetUrl(TeamId teamId) => GetUrl(teamId.Value);
	public static string GetUrl(Guid teamId) => $"/api/v1/teams/{teamId}/nickname";

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task ChangeNickname_ToValidNickname_AsTeamMember_Should_UpdateNicknameInDatabase(TeamRole teamRole)
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(initiatorUser, teamRole, members).Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.Add(initiatorUserUA);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var targetMemberId = team.Members.First(member => member.UserId == initiatorUser.Id).Id;
		var request = new ChangeNicknameRequest
		{
			Nickname = TeamGenerators.GenerateValidNickname()
		};

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			var member = await dbContext
				.Set<TeamMember>()
				.SingleOrDefaultAsync(member => member.Id == targetMemberId);

			member.ShouldNotBeNull();
			member.Nickname.Should().Be(request.Nickname);
		});
	}

	[Fact]
	public async Task ChangeNickname_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([ownerUA, initiatorUserUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var request = new ChangeNicknameRequest
		{
			Nickname = TeamGenerators.GenerateValidNickname()
		};

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task ChangeNickname_InTeamThatDoesNotExist_Should_ResultInNotFound()
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
		var request = new ChangeNicknameRequest
		{
			Nickname = TeamGenerators.GenerateValidNickname()
		};

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(teamId), request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Theory]
	[ClassData(typeof(TeamGenerators.InvalidChangeNicknameRequest))]
	public async Task ChangeNickname_ToInvalidName_Should_ResultInBadRequest_ValidationErrors(InvalidRequest<ChangeNicknameRequest> request)
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(18);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(owner, members, (initiatorUser, TeamRole.Member)).Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([ownerUA, initiatorUserUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var targetMemberId = team.Members.First(member => member.UserId == initiatorUser.Id).Id;

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), request.Request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(request.InvalidProperty);
	}
}
