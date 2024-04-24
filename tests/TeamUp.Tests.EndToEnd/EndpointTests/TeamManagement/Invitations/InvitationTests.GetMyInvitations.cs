using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Invitations;

public sealed class GetMyInvitationsTests(AppFixture app) : InvitationTests(app)
{
	public const string URL = "/api/v1/invitations";

	[Fact]
	public async Task GetMyInvitations_Should_ReturnListOfInvitations()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var teams = TeamGenerators.Team.WithMembers(owner, members).Generate(3);
		var invitations = InvitationGenerators.GenerateUserInvitations(initiatorUser.Id, DateTime.UtcNow.DropMicroSeconds(), teams);

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUserUA, ownerUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, owner]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.AddRange(teams);
			dbContext.Invitations.AddRange(invitations);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		//act
		var response = await Client.GetAsync(URL);

		//assert
		response.Should().Be200Ok();

		var userInvitations = await response.ReadFromJsonAsync<List<InvitationResponse>>();
		invitations.Should().BeEquivalentTo(userInvitations, o => o.ExcludingMissingMembers());
	}

	[Fact]
	public async Task GetMyInvitations_WhenNotInvited_Should_ReturnEmptyList()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var teams = TeamGenerators.Team.WithMembers(owner, members).Generate(3);

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUserUA, ownerUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, owner]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.AddRange(teams);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		//act
		var response = await Client.GetAsync(URL);

		//assert
		response.Should().Be200Ok();

		var invitations = await response.ReadFromJsonAsync<List<InvitationResponse>>();
		invitations.Should().BeEmpty();
	}
}
