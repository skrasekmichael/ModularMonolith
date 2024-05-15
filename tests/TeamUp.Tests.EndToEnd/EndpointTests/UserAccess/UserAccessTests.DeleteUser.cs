using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using TeamUp.TeamManagement.Application.Users;
using TeamUp.TeamManagement.Domain.Aggregates.Events;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Application;
using TeamUp.UserAccess.Application.Abstractions;
using TeamUp.UserAccess.Domain;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.UserAccess;

public sealed class DeleteUserTests(AppFixture app) : UserAccessTests(app)
{
	public const string URL = "/api/v1/users";

	[Fact]
	public async Task DeleteUser_WithCorrectPassword_WhenUserIsNotTeamOwner_Should_DeleteUserAndAssociatedDataFromDatabase()
	{
		//arrange
		var passwordService = App.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerators.GenerateValidPassword();
		var targetUserUA = UserGenerators.User
			.WithPassword(passwordService.HashPassword(rawPassword))
			.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var usersUA = UserGenerators.User.Generate(49);
		usersUA.Add(targetUserUA);
		var users = usersUA.ToUsersTM();

		var teams = TeamGenerators.Team
			.Clone()
			.WithRandomMembers(20, users, 1)
			.WithEventTypes(5)
			.Generate(5);

		var targetTeams = teams.Where(team => team.Members.All(m => m.UserId != targetUserUA.Id)).ToList();
		var targetInvitations = InvitationGenerators.GenerateUserInvitations(targetUserUA.Id, DateTime.UtcNow, targetTeams);

		var user = UserGenerators.User.Generate();
		usersUA.Add(user);
		users.Add(user.ToUserTM());
		var expectedInvitations = InvitationGenerators.GenerateUserInvitations(user.Id, DateTime.UtcNow, teams);

		var events = teams
			.Select(team => EventGenerators.Event
				.Clone()
				.ForTeam(team.Id)
				.WithEventType(team.EventTypes[0].Id)
				.WithRandomEventResponses(team.Members)
				.Generate(5))
			.SelectMany(events => events)
			.ToList();

		var expectedTeamMembers = teams
			.SelectMany(team => team.Members)
			.Where(member => member.UserId != targetUserUA.Id)
			.ToList();
		var expectedEventResponses = events
			.SelectMany(e => e.EventResponses)
			.Where(er => expectedTeamMembers.Any(member => member.Id == er.TeamMemberId))
			.ToList();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange(usersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange(users);
			dbContext.Teams.AddRange(teams);
			dbContext.Events.AddRange(events);
			dbContext.Invitations.AddRange(targetInvitations);
			dbContext.Invitations.AddRange(expectedInvitations);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(targetUserUA);
		Client.DefaultRequestHeaders.Add(UserConstants.HTTP_HEADER_PASSWORD, rawPassword);

		//act
		var response = await Client.DeleteAsync(URL);

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync<UserAccessDbContext>(async dbContext =>
		{
			//only targeted user should be deleted
			var notDeletedUsersUA = await dbContext.Users.ToListAsync();
			notDeletedUsersUA.Should().BeEquivalentTo(usersUA.Without(targetUserUA));
		});

		await WaitForIntegrationEventHandlerAsync<UserDeletedEventHandler>();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			//only targeted user should be deleted
			var notDeletedUsers = await dbContext.Users.ToListAsync();
			notDeletedUsers.Should().BeEquivalentTo(users.Without(targetUser));

			//only team members of target user should be deleted
			var notDeletedTeamMembers = await dbContext.Set<TeamMember>().ToListAsync();
			notDeletedTeamMembers.Should().BeEquivalentTo(expectedTeamMembers);

			//only event responses of targeted user should be deleted
			var notDeletedEventResponses = await dbContext.Set<EventResponse>().ToListAsync();
			notDeletedEventResponses.Should().BeEquivalentTo(expectedEventResponses);

			//only invitations for targeted user should be deleted
			var notDeletedInvitations = await dbContext.Invitations.ToListAsync();
			notDeletedInvitations.Should().BeEquivalentTo(expectedInvitations);
		});
	}

	[Fact]
	public async Task DeleteUser_WithCorrectPassword_WhenUserIsTeamOwnerAndOnlyMember_Should_DeleteUserAndAssociatedDataAndTeamFromDatabase()
	{
		//arrange
		var passwordService = App.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerators.GenerateValidPassword();
		var targetUserUA = UserGenerators.User
			.Clone()
			.WithPassword(passwordService.HashPassword(rawPassword))
			.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var usersUA = UserGenerators.User.Generate(49);
		var users = usersUA.ToUsersTM();

		var teams = TeamGenerators.Team
			.Clone()
			.WithRandomMembers(20, users.With(targetUser), 1)
			.WithEventTypes(5)
			.Generate(4);
		var targetTeam = TeamGenerators.Team
			.Clone()
			.WithOneOwner(targetUser)
			.WithEventTypes(5)
			.Generate();
		var allTeams = teams.With(targetTeam);

		var targetTeams = teams.Where(team => team.Members.All(m => m.UserId != targetUser.Id)).ToList();
		var targetInvitations = InvitationGenerators.GenerateUserInvitations(targetUser.Id, DateTime.UtcNow, targetTeams);

		var user = UserGenerators.User.Generate();
		usersUA.Add(user);
		users.Add(user.ToUserTM());
		var expectedInvitations = InvitationGenerators.GenerateUserInvitations(user.Id, DateTime.UtcNow, allTeams);

		var events = allTeams
			.Select(team => EventGenerators.Event
				.Clone()
				.ForTeam(team.Id)
				.WithEventType(team.EventTypes[0].Id)
				.WithRandomEventResponses(team.Members)
				.Generate(5))
			.SelectMany(events => events)
			.ToList();

		var expectedTeamMembers = teams
			.SelectMany(team => team.Members)
			.Where(member => member.UserId != targetUser.Id)
			.ToList();
		var expectedEventResponses = events
			.SelectMany(e => e.EventResponses)
			.Where(er => expectedTeamMembers.Any(member => member.Id == er.TeamMemberId))
			.ToList();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.Add(targetUserUA);
			dbContext.Users.AddRange(usersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.Add(targetUser);
			dbContext.Users.AddRange(users);
			dbContext.Teams.AddRange(allTeams);
			dbContext.Events.AddRange(events);
			dbContext.Invitations.AddRange(targetInvitations);
			dbContext.Invitations.AddRange(expectedInvitations);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(targetUserUA);
		Client.DefaultRequestHeaders.Add(UserConstants.HTTP_HEADER_PASSWORD, rawPassword);

		//act
		var response = await Client.DeleteAsync(URL);

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync<UserAccessDbContext>(async dbContext =>
		{
			//only targeted user should be deleted
			var notDeletedUsersUA = await dbContext.Users.ToListAsync();
			notDeletedUsersUA.Should().BeEquivalentTo(usersUA);
		});

		await WaitForIntegrationEventHandlerAsync<UserDeletedEventHandler>();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			//only targeted user should be deleted
			var notDeletedUsers = await dbContext.Users.ToListAsync();
			notDeletedUsers.Should().BeEquivalentTo(users);

			//only targeted team should be deleted
			var notDeletedTeams = await dbContext.Teams
				.Include(team => team.EventTypes)
				.ToListAsync();
			notDeletedTeams.Should().BeEquivalentTo(teams);

			//only team members of target user should be deleted
			var notDeletedTeamMembers = await dbContext.Set<TeamMember>().ToListAsync();
			notDeletedTeamMembers.Should().BeEquivalentTo(expectedTeamMembers);

			//only event responses of targeted user should be deleted
			var notDeletedEventResponses = await dbContext.Set<EventResponse>().ToListAsync();
			notDeletedEventResponses.Should().BeEquivalentTo(expectedEventResponses);

			//invitations for targeted user or targeted team should be deleted
			var notDeletedInvitations = await dbContext.Invitations.ToListAsync();
			notDeletedInvitations.Should().BeEquivalentTo(expectedInvitations.Where(invitation => invitation.TeamId != targetTeam.Id));
		});
	}

	[Fact]
	public async Task DeleteUser_WithCorrectPassword_WhenUserIsTeamOwner_Should_DeleteUserAndAssociatedDataFromDatabase_And_ChangeOwnership()
	{
		//arrange
		var passwordService = App.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerators.GenerateValidPassword();
		var targetUserUA = UserGenerators.User
			.Clone()
			.WithPassword(passwordService.HashPassword(rawPassword))
			.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var usersUA = UserGenerators.User.Generate(49);
		var users = usersUA.ToUsersTM();

		var teams = TeamGenerators.Team
			.Clone()
			.WithRandomMembers(25, users.With(targetUser), 1)
			.WithEventTypes(5)
			.Generate(4);
		var targetTeam = TeamGenerators.Team
			.Clone()
			.WithRandomMembers(25, targetUser.With(users), users.Count)
			.WithEventTypes(5)
			.Generate();
		var allTeams = teams.With(targetTeam);

		var targetTeams = teams.Where(team => team.Members.All(m => m.UserId != targetUser.Id)).ToList();
		var targetInvitations = InvitationGenerators.GenerateUserInvitations(targetUser.Id, DateTime.UtcNow, targetTeams);

		var userUA = UserGenerators.User.Generate();
		usersUA.Add(userUA);
		users.Add(userUA.ToUserTM());

		var expectedInvitations = InvitationGenerators.GenerateUserInvitations(userUA.Id, DateTime.UtcNow, allTeams);

		var events = allTeams
			.Select(team => EventGenerators.Event
				.Clone()
				.ForTeam(team.Id)
				.WithEventType(team.EventTypes[0].Id)
				.WithRandomEventResponses(team.Members)
				.Generate(5))
			.SelectMany(events => events)
			.ToList();

		var expectedTeamMembers = allTeams
			.SelectMany(team => team.Members)
			.Where(member => member.UserId != targetUser.Id);
		var expectedEventResponses = events
			.SelectMany(e => e.EventResponses)
			.Where(er => expectedTeamMembers.Any(member => member.Id == er.TeamMemberId))
			.ToList();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.Add(targetUserUA);
			dbContext.Users.AddRange(usersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.Add(targetUser);
			dbContext.Users.AddRange(users);
			dbContext.Teams.AddRange(allTeams);
			dbContext.Events.AddRange(events);
			dbContext.Invitations.AddRange(targetInvitations);
			dbContext.Invitations.AddRange(expectedInvitations);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(targetUserUA);
		Client.DefaultRequestHeaders.Add(UserConstants.HTTP_HEADER_PASSWORD, rawPassword);

		//act
		var response = await Client.DeleteAsync(URL);

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync<UserAccessDbContext>(async dbContext =>
		{
			//only targeted user should be deleted
			var notDeletedUsersUA = await dbContext.Users.ToListAsync();
			notDeletedUsersUA.Should().BeEquivalentTo(usersUA);
		});

		await WaitForIntegrationEventHandlerAsync<UserDeletedEventHandler>();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			//only targeted user should be deleted
			var notDeletedUsers = await dbContext.Users.ToListAsync();
			notDeletedUsers.Should().BeEquivalentTo(users);

			//no teams should be deleted
			var teams = await dbContext.Teams
				.Include(team => team.EventTypes)
				.ToListAsync();
			teams.Should().BeEquivalentTo(allTeams);

			//target member should be removed from team and new owner should be in place
			var team = await dbContext.Teams
				.Include(team => team.Members)
				.Include(team => team.EventTypes)
				.FirstAsync(team => team.Id == targetTeam.Id);
			team.Members
				.Should()
				.HaveCount(targetTeam.Members.Count - 1)
				.And
				.OnlyContain(member =>
					member.UserId != targetUser.Id &&
					targetTeam.Members.Any(m => m.Id == member.Id))
				.And
				.ContainSingle(member => member.Role.IsOwner());

			//only members of targeted user should be deleted
			var notDeletedTeamMembers = await dbContext.Set<TeamMember>().ToListAsync();
			notDeletedTeamMembers.Should().BeEquivalentTo(expectedTeamMembers);

			//only responses of targeted user should be deleted
			var notDeletedEventResponses = await dbContext.Set<EventResponse>().ToListAsync();
			notDeletedEventResponses.Should().BeEquivalentTo(expectedEventResponses);

			//only invitations for targeted user should be deleted
			var notDeletedInvitations = await dbContext.Invitations.ToListAsync();
			notDeletedInvitations.Should().BeEquivalentTo(expectedInvitations);
		});
	}

	[Fact]
	public async Task DeleteUser_WithInCorrectPassword_Should_ResultInUnauthorized()
	{
		//arrange
		var users = UserGenerators.User.Generate(50);
		var targetUser = F.PickRandom(users);

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange(users);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(targetUser);
		Client.DefaultRequestHeaders.Add(UserConstants.HTTP_HEADER_PASSWORD, "incorrect password");

		//act
		var response = await Client.DeleteAsync(URL);

		//assert
		response.Should().Be401Unauthorized();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(AuthenticationErrors.InvalidCredentials);
	}

	[Fact]
	public async Task DeleteUser_ThatDoesNotExist_Should_ResultInNotFound()
	{
		//arrange
		var users = UserGenerators.User.Generate(50);
		var targetUser = UserGenerators.User.Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange(users);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(targetUser);
		Client.DefaultRequestHeaders.Add(UserConstants.HTTP_HEADER_PASSWORD, "whatever password");

		//act
		var response = await Client.DeleteAsync(URL);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UserErrors.UserNotFound);
	}
}
