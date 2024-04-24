using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Invitations;

public sealed class RemoveInvitationTests(AppFixture app) : InvitationTests(app)
{
	public static string GetUrl(InvitationId invitationId) => GetUrl(invitationId.Value);
	public static string GetUrl(Guid invitationId) => $"/api/v1/invitations/{invitationId}";

	[Fact]
	public async Task RemoveInvitation_AsRecipient_Should_RemoveInvitationFromDatabase()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();
		var invitation = InvitationGenerators.GenerateInvitation(initiatorUser.Id, team.Id, DateTime.UtcNow);

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
			dbContext.Teams.Add(team);
			dbContext.Invitations.Add(invitation);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		//act
		var response = await Client.DeleteAsync(GetUrl(invitation.Id));

		//assert
		response.Should().Be200Ok();

		var removedInvitation = await UseDbContextAsync<TeamManagementDbContext, Invitation?>(dbContext => dbContext.Invitations.FindAsync(invitation.Id));
		removedInvitation.Should().BeNull();
	}

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task RemoveInvitation_AsCoordinatorOrHigher_Should_RemoveInvitationFromDatabase(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var targetUserUA = UserGenerators.User.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(initiatorUser, initiatorRole, members).Generate();
		var invitation = InvitationGenerators.GenerateInvitation(targetUser.Id, team.Id, DateTime.UtcNow);

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([targetUserUA, initiatorUserUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([targetUser, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.Add(invitation);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		//act
		var response = await Client.DeleteAsync(GetUrl(invitation.Id));

		//assert
		response.Should().Be200Ok();

		var removedInvitation = await UseDbContextAsync<TeamManagementDbContext, Invitation?>(dbContext => dbContext.Invitations.FindAsync(invitation.Id));
		removedInvitation.Should().BeNull();
	}

	[Fact]
	public async Task RemoveInvitation_AsMember_Should_ResultInForbidden()
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var targetUserUA = UserGenerators.User.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(initiatorUser, TeamRole.Member, members).Generate();
		var invitation = InvitationGenerators.GenerateInvitation(targetUser.Id, team.Id, DateTime.UtcNow);

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([targetUserUA, initiatorUserUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([targetUser, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.Add(invitation);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		//act
		var response = await Client.DeleteAsync(GetUrl(invitation.Id));

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToCancelInvitations);
	}

	[Fact]
	public async Task RemoveInvitation_WhenNotMemberOfTeam_And_NotRecipient_Should_ResultInForbidden()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var targetUserUA = UserGenerators.User.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();
		var invitation = InvitationGenerators.GenerateInvitation(targetUser.Id, team.Id, DateTime.UtcNow);

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([targetUserUA, initiatorUserUA, ownerUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([targetUser, initiatorUser, owner]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.Add(invitation);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		//act
		var response = await Client.DeleteAsync(GetUrl(invitation.Id));

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task RemoveInvitation_ThaDoesNotExist_AsOwner_Should_ResultNotFound()
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(initiatorUser, members).Generate();
		var invitationId = Guid.NewGuid();

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

		//act
		var response = await Client.DeleteAsync(GetUrl(invitationId));

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(InvitationErrors.InvitationNotFound);
	}
}
