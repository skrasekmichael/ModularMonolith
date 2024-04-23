using Microsoft.EntityFrameworkCore;

using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Contracts.Teams.SetMemberRole;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.TeamManagement;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Teams;

public sealed class UpdateTeamMemberRoleTests(AppFixture app) : TeamTests(app)
{
	public static string GetUrl(TeamId teamId, TeamMemberId memberId) => GetUrl(teamId.Value, memberId.Value);
	public static string GetUrl(Guid teamId, Guid memberId) => $"/api/v1/teams/{teamId}/members/{memberId}/role";


	[Theory]
	[InlineData(TeamRole.Admin, TeamRole.Member, TeamRole.Admin)]
	[InlineData(TeamRole.Admin, TeamRole.Member, TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin, TeamRole.Coordinator, TeamRole.Admin)]
	[InlineData(TeamRole.Admin, TeamRole.Coordinator, TeamRole.Member)]
	[InlineData(TeamRole.Admin, TeamRole.Admin, TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin, TeamRole.Admin, TeamRole.Member)]
	[InlineData(TeamRole.Owner, TeamRole.Member, TeamRole.Admin)]
	[InlineData(TeamRole.Owner, TeamRole.Member, TeamRole.Coordinator)]
	[InlineData(TeamRole.Owner, TeamRole.Coordinator, TeamRole.Admin)]
	[InlineData(TeamRole.Owner, TeamRole.Coordinator, TeamRole.Member)]
	[InlineData(TeamRole.Owner, TeamRole.Admin, TeamRole.Coordinator)]
	[InlineData(TeamRole.Owner, TeamRole.Admin, TeamRole.Member)]
	public async Task UpdateTeamRole_OfAdminOrLower_AsOwnerOrAdmin_Should_UpdateTeamMemberRoleInDatabase(TeamRole initiatorRole, TeamRole targetRole, TeamRole newRole)
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var targetUserUA = UserGenerators.User.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(18);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(initiatorUser, initiatorRole, targetUser, targetRole, members).Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUserUA, targetUserUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var targetMemberId = team.Members.First(member => member.UserId == targetUser.Id).Id;
		var request = new UpdateTeamRoleRequest
		{
			Role = newRole
		};

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id, targetMemberId), request);

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			var teamMembers = await dbContext
				.Set<TeamMember>()
				.Where(teamMember => teamMember.TeamId == team.Id)
				.ToListAsync();
			teamMembers.Should().ContainSingle(member => member.Role == TeamRole.Owner);

			var updatedMember = teamMembers.SingleOrDefault(member => member.UserId == targetUser.Id);
			updatedMember.ShouldNotBeNull();
			updatedMember.Role.Should().Be(newRole);

			teamMembers.Except([updatedMember])
				.Should().OnlyContain(member => TeamContainsMemberWithSameRole(team, member));
		});
	}

	[Theory]
	[InlineData(TeamRole.Coordinator, TeamRole.Member, TeamRole.Admin)]
	[InlineData(TeamRole.Coordinator, TeamRole.Member, TeamRole.Coordinator)]
	[InlineData(TeamRole.Coordinator, TeamRole.Coordinator, TeamRole.Admin)]
	[InlineData(TeamRole.Coordinator, TeamRole.Coordinator, TeamRole.Member)]
	[InlineData(TeamRole.Coordinator, TeamRole.Admin, TeamRole.Coordinator)]
	[InlineData(TeamRole.Coordinator, TeamRole.Admin, TeamRole.Member)]
	[InlineData(TeamRole.Member, TeamRole.Member, TeamRole.Admin)]
	[InlineData(TeamRole.Member, TeamRole.Member, TeamRole.Coordinator)]
	[InlineData(TeamRole.Member, TeamRole.Coordinator, TeamRole.Admin)]
	[InlineData(TeamRole.Member, TeamRole.Coordinator, TeamRole.Member)]
	[InlineData(TeamRole.Member, TeamRole.Admin, TeamRole.Coordinator)]
	[InlineData(TeamRole.Member, TeamRole.Admin, TeamRole.Member)]
	public async Task UpdateTeamRole_OfAdminOrLower_AsCoordinatorOrMember_Should_ResultInForbidden(TeamRole initiatorRole, TeamRole targetRole, TeamRole newRole)
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var targetUserUA = UserGenerators.User.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(18);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(initiatorUser, initiatorRole, targetUser, targetRole, members).Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUserUA, targetUserUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUserUA);

		var targetMemberId = team.Members.First(member => member.UserId == targetUser.Id).Id;
		var request = new UpdateTeamRoleRequest
		{
			Role = newRole
		};

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id, targetMemberId), request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToUpdateTeamRoles);
	}

	[Theory]
	[InlineData(TeamRole.Admin, TeamRole.Member)]
	[InlineData(TeamRole.Admin, TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin, TeamRole.Admin)]
	[InlineData(TeamRole.Owner, TeamRole.Member)]
	[InlineData(TeamRole.Owner, TeamRole.Coordinator)]
	[InlineData(TeamRole.Owner, TeamRole.Admin)]
	public async Task UpdateTeamRole_OfOwner_AsOwnerOrAdmin_Should_ResultInBadRequest_DomainError(TeamRole initiatorRole, TeamRole newRole)
	{
		//arrange
		var initiatorUserUA = UserGenerators.User.Generate();
		var initiatorUser = initiatorUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(18);
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

		var targetMemberId = team.Members.First(member => member.Role == TeamRole.Owner).Id;
		var request = new UpdateTeamRoleRequest
		{
			Role = newRole
		};

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id, targetMemberId), request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.CannotChangeTeamOwnersRole);
	}

	[Theory]
	[InlineData((TeamRole)4)]
	[InlineData((TeamRole)99)]
	[InlineData((TeamRole)(-1))]
	public async Task UpdateTeamRole_OfMember_ToInvalidValue_AsOwner_Should_ResultInBadRequest_ValidationErrors(TeamRole newRole)
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var targetUserUA = UserGenerators.User.Generate();
		var targetUser = targetUserUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(owner, members, (targetUser, TeamRole.Member)).Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([ownerUA, targetUserUA]);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.AddRange([owner, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(ownerUA);

		var targetMemberId = team.Members.First(member => member.UserId == targetUser.Id).Id;
		var request = new UpdateTeamRoleRequest
		{
			Role = newRole
		};

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id, targetMemberId), request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(nameof(UpdateTeamRoleRequest.Role));
	}

	[Fact]
	public async Task UpdateTeamRole_OfUnExistingMember_AsOwner_Should_ResultInNotFound()
	{
		//arrange
		var ownerUA = UserGenerators.User.Generate();
		var owner = ownerUA.ToUserTM();

		var membersUA = UserGenerators.User.Generate(19);
		var members = membersUA.ToUsersTM();

		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.Add(ownerUA);
			dbContext.Users.AddRange(membersUA);
			return dbContext.SaveChangesAsync();
		});

		await UseDbContextAsync<TeamManagementDbContext>(dbContext =>
		{
			dbContext.Users.Add(owner);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(ownerUA);

		var targetMemberId = Guid.NewGuid();
		var request = new UpdateTeamRoleRequest
		{
			Role = TeamRole.Member
		};

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id.Value, targetMemberId), request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.MemberNotFound);
	}

	[Fact]
	public async Task UpdateTeamRole_InUnExistingTeam_Should_ResultInNotFound()
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

		var teamId = Guid.NewGuid();
		var targetMemberId = Guid.NewGuid();
		var request = new UpdateTeamRoleRequest
		{
			Role = TeamRole.Member
		};

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(teamId, targetMemberId), request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task UpdateTeamRole_WhenNotMemberOfTeam_Should_ResultInForbidden()
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

		var targetMemberId = team.Members.First(member => member.Role != TeamRole.Owner).Id;
		var request = new UpdateTeamRoleRequest
		{
			Role = TeamRole.Member
		};

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id, targetMemberId), request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}
}
