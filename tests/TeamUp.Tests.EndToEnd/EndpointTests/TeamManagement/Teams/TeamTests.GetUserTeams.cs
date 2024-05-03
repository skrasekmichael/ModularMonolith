using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Teams;

public sealed class GetUserTeamsTests(AppFixture app) : TeamTests(app)
{
	public const string URL = "/api/v1/teams";

	[Fact]
	public async Task GetUserTeams_WhenNotMemberOfTeam_Should_ReturnEmptyList()
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

		//act
		var response = await Client.GetAsync(URL);

		//assert
		response.Should().Be200Ok();

		var teams = await response.ReadFromJsonAsync<List<TeamSlimResponse>>();
		teams.Should().BeEmpty();
	}

	[Fact]
	public async Task GetUserTeams_Should_ReturnTeams_ThatUserIsMemberOf()
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var memberTeams = new TeamRole[] { TeamRole.Owner, TeamRole.Admin, TeamRole.Coordinator, TeamRole.Member }
			.Select(role => TeamGenerators.Team.WithMembers(initiatorUser, role, members).Generate())
			.ToList();
		var expectedTeams = memberTeams.Select(team => new TeamSlimResponse
		{
			TeamId = team.Id,
			Name = team.Name,
		});

		var otherTeam = TeamGenerators.Team.WithOneOwner(members.First());

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
			dbContext.Teams.Add(otherTeam);
			dbContext.Teams.AddRange(memberTeams);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		//act
		var response = await Client.GetAsync(URL);

		//assert
		response.Should().Be200Ok();

		var teams = await response.ReadFromJsonAsync<List<TeamSlimResponse>>();
		teams.Should().BeEquivalentTo(expectedTeams);
	}
}
