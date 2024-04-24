using FluentAssertions.Extensions;

using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Events;

public sealed class GetEventsTests(AppFixture app) : EventTests(app)
{
	public static string GetUrl(TeamId teamId, DateTime? from) => GetUrl(teamId.Value, from);
	public static string GetUrl(Guid teamId, DateTime? from) => from switch
	{
		null => $"/api/v1/teams/{teamId}/events",
		DateTime fromUtc => $"/api/v1/teams/{teamId}/events?fromUtc={fromUtc:o}"
	};

	[Theory]
	[InlineData(TeamRole.Owner)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Member)]
	public async Task GetEvents_InFuture_AsTeamMember_Should_ReturnListOfEvents(TeamRole initiatorRole)
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
		var eventType = team.EventTypes[0];
		var events = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithRandomEventResponses(team.Members)
			.Generate(40);

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

		var expectedEvents = events.Where(e => e.ToUtc > DateTime.UtcNow).ToList();

		//act
		var response = await Client.GetAsync(GetUrl(team.Id, null));

		//assert
		response.Should().Be200Ok();

		var returnedEvents = await response.ReadFromJsonAsync<List<EventSlimResponse>>();
		returnedEvents.ShouldNotBeNull();

		expectedEvents.Should().BeEquivalentTo(returnedEvents, o => o.ExcludingMissingMembers());
		returnedEvents.ForEach(e => EventShouldContainCorrectReplyCount(e, expectedEvents));
	}

	[Fact]
	public async Task GetEvents_FromOldDate_AsOwner_Should_ReturnListOfAllEvents()
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
		var eventType = team.EventTypes[0];
		var events = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithRandomEventResponses(team.Members)
			.Generate(40);

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

		var oldestEventDate = events.Select(e => e.FromUtc).Min();

		//act
		var response = await Client.GetAsync(GetUrl(team.Id, oldestEventDate));

		//assert
		response.Should().Be200Ok();

		var returnedEvents = await response.ReadFromJsonAsync<List<EventSlimResponse>>();
		returnedEvents.ShouldNotBeNull();

		events.Should().BeEquivalentTo(returnedEvents, o => o.ExcludingMissingMembers());
		returnedEvents.ForEach(e => EventShouldContainCorrectReplyCount(e, events));
	}

	[Fact]
	public async Task GetEvents_FromFutureDate_AsOwner_Should_ReturnEmptyList()
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
		var eventType = team.EventTypes[0];
		var events = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithRandomEventResponses(team.Members)
			.Generate(40);

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

		var futureDate = events.Select(e => e.ToUtc).Max().AddHours(1).AsUtc();

		//act
		var response = await Client.GetAsync(GetUrl(team.Id, futureDate));

		//assert
		response.Should().Be200Ok();

		var returnedEvents = await response.ReadFromJsonAsync<List<EventSlimResponse>>();
		returnedEvents.Should().BeEmpty();
	}

	[Fact]
	public async Task GetEvents_FromUnExistingTeam_AsOwner_Should_ResultInNotFound()
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

		var teamId = Guid.NewGuid();

		//act
		var response = await Client.GetAsync(GetUrl(teamId, null));

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task GetEvents_WhenNotMemberOfTeam_AsOwner_Should_ResultInForbidden()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team
			.WithMembers(owner, members)
			.WithEventTypes(5)
			.Generate();
		var eventType = team.EventTypes[0];
		var events = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithRandomEventResponses(team.Members)
			.Generate(40);

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
			dbContext.Events.AddRange(events);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		//act
		var response = await Client.GetAsync(GetUrl(team.Id, null));

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}
}
