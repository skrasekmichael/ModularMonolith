using Microsoft.EntityFrameworkCore;

using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Teams;

public sealed class DeleteTeamTests(AppFixture app) : TeamTests(app)
{
	public static string GetUrl(TeamId teamId) => GetUrl(teamId.Value);
	public static string GetUrl(Guid teamId) => $"/api/v1/teams/{teamId}";

	[Fact]
	public async Task DeleteTeam_AsOwner_Should_DeleteTeamAndAssociatedDataInDatabase()
	{
		//arrange
		var usersUA = UserGenerators.User.Generate(80);
		var users = usersUA.ToUsersTM();

		var teams = TeamGenerators.Team
			.WithRandomMembers(20, users)
			.WithEventTypes(5)
			.Generate(4);

		var userUA = UserGenerators.User.Generate();
		usersUA.Add(userUA);
		var user = userUA.ToUserTM();
		users.Add(user);

		var invitations = InvitationGenerators.GenerateUserInvitations(user.Id, DateTime.UtcNow, teams);

		var teamEvents = teams.Select(team =>
		{
			return EventGenerators.Event
				.ForTeam(team.Id)
				.WithEventType(team.EventTypes[0].Id)
				.WithRandomEventResponses(team.Members)
				.Generate(15);
		}).ToList();
		var events = teamEvents.SelectMany(events => events);

		var targetTeamIndex = F.Random.Int(0, teams.Count - 1);
		var targetTeam = teams[targetTeamIndex];
		var teamOwner = targetTeam.Members.Single(member => member.Role.IsOwner());
		var initiatorUserUA = usersUA.Single(user => user.Id == teamOwner.UserId);

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
			dbContext.Invitations.AddRange(invitations);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		//act
		var response = await Client.DeleteAsync(GetUrl(targetTeam.Id));

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			//no users deleted
			var notDeletedUsers = await dbContext.Users.ToListAsync();
			notDeletedUsers.Should().BeEquivalentTo(users);

			//only team, team members and event types from target team were deleted
			var notDeletedTeams = await dbContext.Teams
				.Include(team => team.Members)
				.Include(team => team.EventTypes)
				.ToListAsync();
			notDeletedTeams.Should().BeEquivalentTo(teams.Without(targetTeam));

			//only events and event responses from target team were deleted
			var notDeletedEvents = await dbContext.Events
				.Include(e => e.EventResponses)
				.ToListAsync();
			notDeletedEvents.Should().BeEquivalentTo(events.Except(teamEvents[targetTeamIndex]));

			//only invitation to targeted team were deleted
			var notDeletedInvitations = await dbContext.Invitations.ToListAsync();
			notDeletedInvitations.Should().BeEquivalentTo(invitations.Where(invitation => invitation.TeamId != targetTeam.Id));
		});
	}

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	public async Task DeleteTeam_AsAdminOrLower_Should_ResultInForbidden(TeamRole teamRole)
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(18);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(owner, members, (initiatorUser, teamRole)).Generate();

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

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id));

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToDeleteTeam);
	}

	[Fact]
	public async Task DeleteTeam_ThatDoesNotExist_Should_ResultInNotFound()
	{
		//arrange
		var userUA = UserGenerators.User.Generate();
		var user = userUA.ToUserTM();

		var teamId = Guid.NewGuid();

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
		var response = await Client.DeleteAsync(GetUrl(teamId));

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task DeleteTeam_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(18);
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

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id));

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}
}
