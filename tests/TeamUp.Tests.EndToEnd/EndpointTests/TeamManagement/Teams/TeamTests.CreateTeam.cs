using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Infrastructure.Services;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Contracts.Teams.CreateTeam;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Users;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Teams;

public sealed class CreateTeamTests(AppFixture app) : TeamTests(app)
{
	public const string URL = "/api/v1/teams";

	[Fact]
	public async Task CreateTeam_WhenOwns4Teams_Should_CreateNewTeamInDatabase_WithOneTeamOwner()
	{
		//arrange
		var userUA = UserGenerators.User.Generate();
		var user = userUA.ToUserTM();

		var ownedTeams = TeamGenerators.Team
			.WithOneOwner(user)
			.Generate(TeamConstants.MAX_NUMBER_OF_OWNED_TEAMS - 1);

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.Add(userUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.Add(user);
			dbContext.Teams.AddRange(ownedTeams);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(userUA);

		var createTeamRequest = new CreateTeamRequest
		{
			Name = TeamGenerators.GenerateValidTeamName()
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, createTeamRequest);

		//assert
		response.Should().Be201Created();

		var teamId = await response.ReadFromJsonAsync<TeamId>();
		teamId.ShouldNotBeNull();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			var team = await dbContext.Teams
				.AsSplitQuery()
				.Include(team => team.Members)
				.Include(team => team.EventTypes)
				.FirstOrDefaultAsync(team => team.Id == teamId);

			team.ShouldNotBeNull();
			team.Name.Should().Be(createTeamRequest.Name);
			team.EventTypes.Should().BeEmpty();

			var tm = team.Members[0];
			tm.UserId.Should().Be(user.Id);
			tm.TeamId.Should().Be(teamId);
			tm.Nickname.Should().Be(user.Name);
			tm.Role.Should().Be(TeamRole.Owner);
		});
	}

	[Theory]
	[ClassData(typeof(TeamGenerators.InvalidCreateTeamRequest))]
	public async Task CreateTeam_WithInvalidName_Should_ResultInBadRequest_ValidationError(InvalidRequest<CreateTeamRequest> request)
	{
		//arrange
		var userUA = UserGenerators.User.Generate();
		var user = userUA.ToUserTM();

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
		var response = await Client.PostAsJsonAsync(URL, request.Request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(request.InvalidProperty);
	}


	[Fact]
	public async Task CreateTeam_AsUnExistingUser_Should_ResultInNotFound()
	{
		//arrange
		var userUA = UserGenerators.User.Generate();

		Authenticate(userUA);

		var createTeamRequest = new CreateTeamRequest
		{
			Name = TeamGenerators.GenerateValidTeamName()
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, createTeamRequest);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UserErrors.UserNotFound);
	}

	[Fact]
	public async Task CreateTeam_WhenOwns5Teams_Should_ResultInBadRequest_DomainError()
	{
		//arrange
		var userUA = UserGenerators.User.Generate();
		var user = userUA.ToUserTM();

		var ownedTeams = TeamGenerators.Team
			.WithOneOwner(user)
			.Generate(TeamConstants.MAX_NUMBER_OF_OWNED_TEAMS);

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.Add(userUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.Add(user);
			dbContext.Teams.AddRange(ownedTeams);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(userUA);

		var createTeamRequest = new CreateTeamRequest
		{
			Name = TeamGenerators.GenerateValidTeamName()
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, createTeamRequest);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.CannotOwnSoManyTeams);
	}

	[Fact]
	public async Task CreateTeam_WhenOwns4Teams_AndConcurrentCreateTeamCompletes_Should_ResultInConflict()
	{
		//arrange
		var userUA = UserGenerators.User.Generate();
		var user = userUA.ToUserTM();

		var ownedTeams = TeamGenerators.Team
			.WithOneOwner(user)
			.Generate(TeamConstants.MAX_NUMBER_OF_OWNED_TEAMS - 1);

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.Add(userUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.Add(user);
			dbContext.Teams.AddRange(ownedTeams);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(userUA);

		var createTeamRequest = new CreateTeamRequest
		{
			Name = TeamGenerators.GenerateValidTeamName()
		};

		//act
		var (responseA, responseB) = await RunConcurrentRequestsAsync<TeamManagementModuleId>(
			() => Client.PostAsJsonAsync(URL, createTeamRequest),
			() => Client.PostAsJsonAsync(URL, createTeamRequest)
		);

		//assert
		responseA.Should().Be201Created();
		responseB.Should().Be409Conflict();

		var teamId = await responseA.ReadFromJsonAsync<TeamId>();
		teamId.ShouldNotBeNull();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			var ownedTeamsCount = await dbContext
				.Set<TeamMember>()
				.Where(member => member.UserId == user.Id && member.Role == TeamRole.Owner)
				.CountAsync();

			ownedTeamsCount.Should().Be(TeamConstants.MAX_NUMBER_OF_OWNED_TEAMS);

			var team = await dbContext.Teams
				.AsSplitQuery()
				.Include(team => team.Members)
				.Include(team => team.EventTypes)
				.FirstOrDefaultAsync(team => team.Id == teamId);

			team.ShouldNotBeNull();
			team.Name.Should().Be(createTeamRequest.Name);
			team.EventTypes.Should().BeEmpty();

			var tm = team.Members[0];
			tm.UserId.Should().Be(user.Id);
			tm.TeamId.Should().Be(teamId);
			tm.Nickname.Should().Be(user.Name);
			tm.Role.Should().Be(TeamRole.Owner);
		});

		var problemDetails = await responseB.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UnitOfWork<TeamManagementDbContext, TeamManagementModuleId>.ConcurrencyError);
	}
}
