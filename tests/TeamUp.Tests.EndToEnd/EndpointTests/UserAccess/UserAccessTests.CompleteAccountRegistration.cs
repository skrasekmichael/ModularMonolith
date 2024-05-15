using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Domain;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.UserAccess;

public sealed class CompleteAccountRegistrationTests(AppFixture app) : UserAccessTests(app)
{
	public static string GetUrl(UserId userId) => GetUrl(userId.Value);
	public static string GetUrl(Guid userId) => $"/api/v1/users/{userId}/generated/complete";

	[Theory]
	[InlineData(UserState.Activated)]
	[InlineData(UserState.NotActivated)]
	public async Task CompleteAccountRegistration_ThatIsNotGenerated_Should_ResultInBadRequest_DomainError(UserState status)
	{
		//arrange
		var user = UserGenerators.User
			.Clone()
			.WithStatus(status)
			.Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync();
		});

		//act
		Client.DefaultRequestHeaders.Add(UserConstants.HTTP_HEADER_PASSWORD, "password");
		var response = await Client.PostAsync(GetUrl(user.Id), null);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UserErrors.CannotCompleteRegistrationOfNonGeneratedAccount);
	}

	[Fact]
	public async Task CompleteAccountRegistration_ThatIsGenerated_Should_ResultInSettingPasswordAndUpdatingUserStatusInDatabase()
	{
		//arrange
		var user = UserGenerators.User
			.Clone()
			.WithStatus(UserState.Generated)
			.Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync();
		});

		//act
		Client.DefaultRequestHeaders.Add(UserConstants.HTTP_HEADER_PASSWORD, "password");
		var response = await Client.PostAsync(GetUrl(user.Id), null);

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync<UserAccessDbContext>(async dbContext =>
		{
			var activatedUser = await dbContext.Users.FindAsync(user.Id);
			activatedUser.ShouldNotBeNull();
			activatedUser.State.Should().Be(UserState.Activated);
		});
	}
}
