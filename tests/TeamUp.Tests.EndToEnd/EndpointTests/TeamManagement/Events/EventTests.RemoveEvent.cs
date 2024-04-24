using Microsoft.EntityFrameworkCore;

using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Events;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Events;

public sealed class RemoveEventTests(AppFixture app) : EventTests(app)
{
	public static string GetUrl(TeamId teamId, EventId eventId) => GetUrl(teamId.Value, eventId.Value);
	public static string GetUrl(Guid teamId, Guid eventId) => $"/api/v1/teams/{teamId}/events/{eventId}";

	[Theory]
	[InlineData(TeamRole.Owner)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Coordinator)]
	public async Task RemoveEvent_AsCoordinatorOrHigher_Should_RemoveEventAndEventResponsesFromDatabase(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, initiatorRole, members)
			.WithEventTypes(5)
			.Generate();
		var events = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(team.EventTypes[0].Id)
			.WithRandomEventResponses(team.Members)
			.Generate(10);

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
			dbContext.Events.AddRange(events);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var targetEvent = F.PickRandom(events);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id, targetEvent.Id));

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			var rest = await dbContext.Events
				.Include(e => e.EventResponses)
				.ToListAsync();

			rest.Should().BeEquivalentTo(events.Except([targetEvent]));
		});
	}

	[Fact]
	public async Task RemoveEvent_AsMember_Should_ResultInForbidden()
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, TeamRole.Member, members)
			.WithEventTypes(5)
			.Generate();
		var events = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(team.EventTypes[0].Id)
			.WithRandomEventResponses(team.Members)
			.Generate(10);

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
			dbContext.Events.AddRange(events);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var targetEvent = F.PickRandom(events);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id, targetEvent.Id));

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToDeleteEvents);
	}

	[Fact]
	public async Task RemoveEvent_ThatDoesNotExist_AsOwner_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var events = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(team.EventTypes[0].Id)
			.WithRandomEventResponses(team.Members)
			.Generate(10);

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
			dbContext.Events.AddRange(events);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var targetEventId = EventId.New();

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id, targetEventId));

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(EventErrors.EventNotFound);
	}

	[Fact]
	public async Task RemoveEvent_FromUnExistingTeam_Should_ResultInNotFound()
	{
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

		var targetTeamId = TeamId.New();
		var targetEventId = EventId.New();

		//act
		var response = await Client.DeleteAsync(GetUrl(targetTeamId, targetEventId));

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task RemoveEvent_OfAnotherTeam_AsOwner_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team1 = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var team1Events = EventGenerators.Event
			.ForTeam(team1.Id)
			.WithEventType(team1.EventTypes[0].Id)
			.WithRandomEventResponses(team1.Members)
			.Generate(10);

		var team2 = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var team2Events = EventGenerators.Event
			.ForTeam(team2.Id)
			.WithEventType(team2.EventTypes[0].Id)
			.WithRandomEventResponses(team2.Members)
			.Generate(10);

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
			dbContext.Teams.AddRange([team1, team2]);
			dbContext.Events.AddRange(team1Events);
			dbContext.Events.AddRange(team2Events);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var targetTeamId = team1.Id;
		var targetEvent = F.PickRandom(team2Events);

		//act
		var response = await Client.DeleteAsync(GetUrl(targetTeamId, targetEvent.Id));

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(EventErrors.EventNotFound);
	}
}
