﻿using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.UserAccess;

public sealed class ActivateAccountTests(AppFixture app) : UserAccessTests(app)
{
	public static string GetUrl(UserId userId) => GetUrl(userId.Value);
	public static string GetUrl(Guid userId) => $"/api/v1/users/{userId}/activate";

	[Fact]
	public async Task ActivateAccount_Should_SetUserStatusAsActivatedInDatabase()
	{
		//arrange
		var user = UserGenerators.User
			.Clone()
			.WithStatus(UserState.NotActivated)
			.Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync();
		});

		//act
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
