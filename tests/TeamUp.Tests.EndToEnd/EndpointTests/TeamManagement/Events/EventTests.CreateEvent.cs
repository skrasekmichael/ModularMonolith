using TeamUp.TeamManagement.Contracts.Events;
using TeamUp.TeamManagement.Contracts.Events.CreateEvent;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Events;

public sealed class CreateEventTests(AppFixture app) : EventTests(app)
{
	public static string GetUrl(TeamId teamId) => GetUrl(teamId.Value);
	public static string GetUrl(Guid teamId) => $"/api/v1/teams/{teamId}/events";

	[Theory]
	[InlineData(TeamRole.Owner)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Coordinator)]
	public async Task CreateEvent_AsCoordinatorOrHigher_Should_CreateNewEventInDatabase(TeamRole initiatorRole)
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

		var request = EventGenerators.ValidCreateEventRequest
			.WithEventType(team.EventTypes[0].Id)
			.WithTime(DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(4).AddHours(1))
			.Generate();

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be201Created();

		var eventId = await response.ReadFromJsonAsync<EventId>();
		eventId.ShouldNotBeNull();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			var createdEvent = await dbContext.Events.FindAsync(eventId);

			createdEvent.ShouldNotBeNull();
			createdEvent.Status.Should().Be(EventStatus.Open);
			createdEvent.TeamId.Should().Be(team.Id);
			createdEvent.Should().BeEquivalentTo(request, o => o.ExcludingMissingMembers());
		});
	}

	[Fact]
	public async Task CreateEvent_AsMember_Should_ResultInForbidden()
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

		var request = EventGenerators.ValidCreateEventRequest
			.WithEventType(team.EventTypes[0].Id)
			.WithTime(DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(4).AddHours(1))
			.Generate();

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToCreateEvents);
	}

	[Fact]
	public async Task CreateEvent_WhenNotMember_Should_ResultInForbidden()
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

		var request = EventGenerators.ValidCreateEventRequest
			.WithEventType(team.EventTypes[0].Id)
			.WithTime(DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(4).AddHours(1))
			.Generate();

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task CreateEvent_ForUnExistingTeam_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var teamId = Guid.NewGuid();
		var eventTypeId = EventTypeId.New();

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

		var request = EventGenerators.ValidCreateEventRequest
			.WithEventType(eventTypeId)
			.WithTime(DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(4).AddHours(1))
			.Generate();

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(teamId), request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task CreateEvent_WithUnExistingEventType_Should_ResultInNotFound()
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

		var request = EventGenerators.ValidCreateEventRequest
			.WithEventType(EventTypeId.New())
			.WithTime(DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(4).AddHours(1))
			.Generate();

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.EventTypeNotFound);
	}

	[Theory]
	[ClassData(typeof(EventGenerators.InvalidCreateEventRequest))]
	public async Task CreateEvent_WithInvalidParameters_AsOwner_Should_ResultInBadRequest_ValidationError(InvalidRequest<CreateEventRequest> invalidRequest)
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

		var request = invalidRequest.Request with
		{
			EventTypeId = team.EventTypes[0].Id,
		};

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(invalidRequest.InvalidProperty);
	}
}
