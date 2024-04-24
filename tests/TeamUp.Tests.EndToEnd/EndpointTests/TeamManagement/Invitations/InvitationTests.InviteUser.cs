using Microsoft.EntityFrameworkCore;

using TeamUp.TeamManagement.Contracts.Invitations.InviteUser;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Invitations;

public sealed class InviteUserTests(AppFixture app) : InvitationTests(app)
{
	public const string URL = "/api/v1/invitations";

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task InviteUser_ThatIsActivated_AsCoordinatorOrHigher_Should_CreateInvitationInDatabase_And_SendInvitationEmail(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var targetUserUA = UserGenerators.User.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(initiatorUser, initiatorRole, members).Generate();

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
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.Should().Be202Accepted();

		await WaitForIntegrationEventHandlerAsync<TeamUp.TeamManagement.Application.Invitations.CreateInvitationRequestCreatedEventHandler>();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			var invitation = await dbContext.Invitations.FirstAsync();
			invitation.ShouldNotBeNull();
			invitation.TeamId.Should().Be(team.Id);
			invitation.RecipientId.Should().Be(targetUser.Id);
		});

		await WaitForIntegrationEventHandlerAsync<Notifications.Application.Email.EmailCreatedEventHandler>();

		Inbox.Should().Contain(email => email.EmailAddress == targetUser.Email);
	}

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task InviteUser_ThatIsNotRegistered_AsCoordinatorOrHigher_Should_CreateInvitationInDatabase_And_GenerateNewUser_And_SendInvitationEmail(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(initiatorUser, initiatorRole, members).Generate();

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

		var targetEmail = F.Internet.Email();
		var request = new InviteUserRequest
		{
			Email = targetEmail,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.Should().Be202Accepted();

		await WaitForIntegrationEventHandlerAsync<TeamUp.TeamManagement.Application.Users.UserCreatedEventHandler>();
		await WaitForIntegrationEventHandlerAsync<TeamUp.TeamManagement.Application.Invitations.CreateInvitationRequestCreatedEventHandler>();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			var invitation = await dbContext.Invitations.FirstAsync();
			invitation.TeamId.Should().Be(team.Id);

			var user = await dbContext.Users.FindAsync(invitation.RecipientId);
			user.ShouldNotBeNull();
			user.Email.Should().Be(targetEmail);

			await UseDbContextAsync<UserAccessDbContext>(async dbContext =>
			{
				var user = await dbContext.Users.FindAsync(invitation.RecipientId);
				user.ShouldNotBeNull();
				user.Email.Should().Be(targetEmail);
				user.State.Should().Be(UserState.Generated);
			});
		});

		await WaitForIntegrationEventHandlerAsync<Notifications.Application.Email.EmailCreatedEventHandler>();

		Inbox.Should().Contain(email => email.EmailAddress == targetEmail);
	}

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task InviteUser_ThatIsAlreadyInTeam_AsCoordinatorOrHigher_Should_ResultInBadRequest_DomainError(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(initiatorUser, initiatorRole, members).Generate();
		var targetUser = members.First();

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

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.CannotInviteUserThatIsTeamMember);
	}

	[Fact]
	public async Task InviteUser_AsTeamMember_Should_ResultInForbidden()
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var targetUserUA = UserGenerators.User.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(initiatorUser, TeamRole.Member, members).Generate();

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
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToInviteTeamMembers);
	}

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task InviteUser_ThatIsAlreadyInvited_AsCoordinatorOrHigher_Should_ResultInConflict(TeamRole initiatorRole)
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

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.Should().Be409Conflict();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(InvitationErrors.UserIsAlreadyInvited);
	}

	[Fact]
	public async Task InviteUser_WhenNotMemberOfTeam_Should_ResultInForbidden()
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

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([ownerUA, targetUserUA, initiatorUserUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([owner, targetUser, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task InviteUser_ToUnExistingTeam_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var targetUserUA = UserGenerators.User.Generate();
		var targetUser = targetUserUA.ToUserTM();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([targetUserUA, initiatorUserUA]);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([targetUser, initiatorUser]);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = TeamId.New()
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Theory]
	[ClassData(typeof(InvitationGenerators.InvalidInviteUserRequest))]
	public async Task InviteUser_WithInvalidRequest_Should_ResultInBadRequest_ValidationErrors(InvalidRequest<InviteUserRequest> request)
	{
		//arrange
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.Add(initiatorUserUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		//act
		var response = await Client.PostAsJsonAsync(URL, request.Request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(request.InvalidProperty);
	}

	[Fact]
	public async Task InviteUser_AsOwner_WhenTeamIsFull_Should_ResultInBadRequest_DomainError()
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var targetUserUA = UserGenerators.User.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(TeamConstants.MAX_TEAM_CAPACITY - 1);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(initiatorUser, members).Generate();

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
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.MaximumCapacityReached);
	}
}
