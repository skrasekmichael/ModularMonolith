using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Events;

public sealed class GetEventTests(AppFixture app) : EventTests(app)
{
	public static string GetUrl(TeamId teamId, EventId eventId) => GetUrl(teamId.Value, eventId.Value);
	public static string GetUrl(Guid teamId, Guid eventId) => $"/api/v1/teams/{teamId}/events/{eventId}";

	[Theory]
	[InlineData(TeamRole.Owner)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Member)]
	public async Task GetEvent_AsTeamMember_Should_ReturnEvent(TeamRole initiatorRole)
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
		var @event = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithRandomEventResponses(team.Members)
			.Generate();

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
			dbContext.Events.Add(@event);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		//act
		var response = await Client.GetAsync(GetUrl(team.Id, @event.Id));

		//assert
		response.Should().Be200Ok();

		var returnedEvent = await response.ReadFromJsonAsync<EventResponse>();
		returnedEvent.ShouldNotBeNull();

		returnedEvent.EventType.Should().Be(eventType.Name);
		@event.Should().BeEquivalentTo(returnedEvent, o => o.ExcludingMissingMembers());
		returnedEvent.EventResponses.Should().Contain(err => ResponseIsFromMemberWithCorrectNickname(err, team) && ResponseHasCorrectReply(err, @event));
	}

	[Fact]
	public async Task GetEvent_ThatDoesNotExist_AsOwner_Should_ReturnNotFound()
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

		var eventId = Guid.NewGuid();

		//act
		var response = await Client.GetAsync(GetUrl(team.Id.Value, eventId));

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamUp.TeamManagement.Domain.Aggregates.Events.EventErrors.EventNotFound);
	}

	[Fact]
	public async Task GetEvent_InUnExistingTeam_Should_ReturnNotFound()
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
		var eventId = Guid.NewGuid();

		//act
		var response = await Client.GetAsync(GetUrl(teamId, eventId));

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task GetEvent_WhenNotMemberOfTeam_Should_ReturnForbidden()
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
		var @event = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithRandomEventResponses(team.Members)
			.Generate();

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
			dbContext.Events.Add(@event);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		//act
		var response = await Client.GetAsync(GetUrl(team.Id, @event.Id));

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}
}
