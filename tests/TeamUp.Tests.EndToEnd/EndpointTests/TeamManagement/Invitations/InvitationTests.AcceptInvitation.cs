using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Infrastructure.Services;
using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Invitations;

public sealed class AcceptInvitationTests(AppFixture app) : InvitationTests(app)
{
	public static string GetUrl(InvitationId invitationId) => GetUrl(invitationId.Value);
	public static string GetUrl(Guid invitationId) => $"/api/v1/invitations/{invitationId}/accept";

	[Fact]
	public async Task AcceptInvitation_ThatIsValid_AsRecipient_Should_RemoveInvitationFromDatabase_And_AddUserAsMemberToTeamInDatabase()
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
		var response = await Client.PostAsync(GetUrl(invitation.Id), null);

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			var teamMembers = await dbContext
				.Set<TeamMember>()
				.Where(member => member.TeamId == team.Id)
				.ToListAsync();

			teamMembers.Should().HaveCount(21);

			var teamMember = teamMembers.SingleOrDefault(member => member.UserId == initiatorUser.Id);
			teamMember.ShouldNotBeNull();
			teamMember.Role.Should().Be(TeamRole.Member);

			var acceptedInvitation = await dbContext.Invitations.FindAsync(invitation.Id);
			acceptedInvitation.Should().BeNull();
		});
	}

	[Fact]
	public async Task AcceptInvitation_ThatExpired_AsRecipient_Should_ResultInBadRequest_DomainError()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();
		var invitation = InvitationGenerators.GenerateInvitation(initiatorUser.Id, team.Id, DateTime.UtcNow.AddDays(-5));

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
		var response = await Client.PostAsync(GetUrl(invitation.Id), null);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(InvitationErrors.InvitationExpired);
	}

	[Fact]
	public async Task AcceptInvitation_ThatDoesNotExist_AsRecipient_Should_ResultInNotFound()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();
		var invitationId = Guid.NewGuid();

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
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		//act
		var response = await Client.PostAsync(GetUrl(invitationId), null);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(InvitationErrors.InvitationNotFound);
	}

	[Fact]
	public async Task AcceptInvitation_ForAnotherUser_Should_ResultInForbidden()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var targetUserUA = UserGenerators.User.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();
		var invitation = InvitationGenerators.GenerateInvitation(targetUser.Id, team.Id, DateTime.UtcNow.AddDays(-5));

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUserUA, ownerUA, targetUserUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, owner, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.Add(invitation);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		//act
		var response = await Client.PostAsync(GetUrl(invitation.Id), null);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(InvitationErrors.UnauthorizedToAcceptInvitation);
	}

	[Fact]
	public async Task AcceptInvitation_ThatIsValid_AsRecipient_WhenTeamIsFull_Should_ResultInBadRequest_DomainError()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(TeamConstants.MAX_TEAM_CAPACITY - 1);
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
		var response = await Client.PostAsync(GetUrl(invitation.Id), null);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.MaximumCapacityReached);
	}

	[Fact]
	public async Task AcceptInvitation_ThatIsValid_AsRecipient_ForLastEmptySpot_WhenConcurrentInvitationToSameTeamHasBeenAccepted_Should_ResultInConflict()
	{
		//arrange
		var userAUA = UserGenerators.User.Generate();
		var userA = userAUA.ToUserTM();

		var userBUA = UserGenerators.User.Generate();
		var userB = userBUA.ToUserTM();

		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(TeamConstants.MAX_TEAM_CAPACITY - 2);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();
		var invitationA = InvitationGenerators.GenerateInvitation(userA.Id, team.Id, DateTime.UtcNow);
		var invitationB = InvitationGenerators.GenerateInvitation(userB.Id, team.Id, DateTime.UtcNow);

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([userAUA, userBUA, ownerUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([userA, userB, owner]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.AddRange([invitationA, invitationB]);
			return dbContext.SaveChangesAsync();
		});

		//act
		var (responseA, responseB) = await RunConcurrentRequestsAsync<TeamManagementModuleId>(
			() =>
			{
				Authenticate(userAUA);
				return Client.PostAsync(GetUrl(invitationA.Id), null);
			},
			() =>
			{
				Authenticate(userBUA);
				return Client.PostAsync(GetUrl(invitationB.Id), null);
			}
		);

		//assert
		responseA.Should().Be200Ok();
		responseB.Should().Be409Conflict();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			var teamMembers = await dbContext
				.Set<TeamMember>()
				.Where(member => member.TeamId == team.Id)
				.ToListAsync();

			teamMembers.Should().HaveCount(TeamConstants.MAX_TEAM_CAPACITY);

			var teamMember = teamMembers.SingleOrDefault(member => member.UserId == userA.Id);
			teamMember.ShouldNotBeNull();
			teamMember.Role.Should().Be(TeamRole.Member);

			var acceptedInvitation = await dbContext.Invitations.FindAsync(invitationA.Id);
			acceptedInvitation.Should().BeNull();
		});

		var problemDetails = await responseB.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UnitOfWork<TeamManagementDbContext, TeamManagementModuleId>.ConcurrencyError);
	}
}
