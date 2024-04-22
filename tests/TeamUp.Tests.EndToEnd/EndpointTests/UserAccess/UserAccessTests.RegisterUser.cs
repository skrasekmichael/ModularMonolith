using System.Net.Http.Json;

using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Contracts.CreateUser;
using TeamUp.UserAccess.Domain;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.UserAccess;

public sealed class RegisterUserTests(AppFixture app) : UserAccessTests(app)
{
	public const string URL = "/api/v1/users/register";

	[Fact]
	public async Task RegisterUser_Should_CreateNewUserInDatabase_And_SendActivationEmail()
	{
		//arrange
		var request = UserGenerators.ValidRegisterUserRequest.Generate();

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.Should().Be201Created();

		var userId = await response.ReadFromJsonAsync<UserId>();
		userId.ShouldNotBeNull();

		await UseDbContextAsync<UserAccessDbContext>(async dbContext =>
		{
			var user = await dbContext.Users.FindAsync(userId);
			user.ShouldNotBeNull();

			user.Name.Should().Be(request.Name);
			user.Email.Should().Be(request.Email);
			user.Password.Should().NotBeEquivalentTo(request.Password);
		});

		await WaitForIntegrationEventHandlerAsync<TeamManagement.Application.Users.UserCreatedEventHandler>();

		await UseDbContextAsync<TeamManagementDbContext>(async dbContext =>
		{
			var user = await dbContext.Users.FindAsync(userId);
			user.ShouldNotBeNull();

			user.Name.Should().Be(request.Name);
			user.Email.Should().Be(request.Email);
			user.NumberOfOwnedTeams.Should().Be(0);
		});

		await WaitForIntegrationEventHandlerAsync<Notifications.Application.Email.EmailCreatedEventHandler>();

		Inbox.Should().Contain(email => email.EmailAddress == request.Email);
	}

	[Fact]
	public async Task RegisterUser_WithAlreadyUsedEmail_Should_ResultInConflict()
	{
		//arrange
		var user = UserGenerators.User.Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		var request = new RegisterUserRequest()
		{
			Email = user.Email,
			Name = F.Internet.UserName(),
			Password = UserGenerators.GenerateValidPassword(),
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.Should().Be409Conflict();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UserErrors.ConflictingEmail);
	}

	[Theory]
	[ClassData(typeof(UserGenerators.InvalidRegisterUserRequests))]
	public async Task RegisterUser_WithInvalidProperties_Should_ResultInValidationErrors_BadRequest(InvalidRequest<RegisterUserRequest> request)
	{
		//arrange
		//act
		var response = await Client.PostAsJsonAsync(URL, request.Request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(request.InvalidProperty);
	}
}
