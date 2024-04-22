using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.Extensions.DependencyInjection;

using TeamUp.Tests.Common.DataGenerators.UserAccess;
using TeamUp.UserAccess.Application;
using TeamUp.UserAccess.Application.Abstractions;
using TeamUp.UserAccess.Contracts.Login;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.Tests.EndToEnd.EndpointTests.UserAccess;

public sealed class LoginTests(AppFixture app) : UserAccessTests(app)
{
	public const string URL = "/api/v1/users/login";

	[Fact]
	public async Task Login_AsActivatedUser_Should_GenerateValidJwtToken()
	{
		//arrange
		var passwordService = App.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerators.GenerateValidPassword();
		var user = UserGenerators.User
			.Clone()
			.WithPassword(passwordService.HashPassword(rawPassword))
			.Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync();
		});

		var request = new LoginRequest
		{
			Email = user.Email,
			Password = rawPassword
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.Should().Be200Ok();

		var token = await response.ReadFromJsonAsync<string>();
		token.Should().NotBeNullOrEmpty();

		var handler = new JwtSecurityTokenHandler();
		var jwt = handler.ReadJwtToken(token);

		jwt.ShouldNotBeNull();
		jwt.ValidTo.Ticks.Should().BeGreaterThan(DateTime.UtcNow.Ticks);
		jwt.Claims.Select(claim => (claim.Type, claim.Value))
			.Should()
			.Contain([
				(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
				(ClaimTypes.Name, user.Name),
				(ClaimTypes.Email, user.Email)
			]);
	}

	[Fact]
	public async Task Login_AsInactivatedUser_Should_ResultInUnauthorized()
	{
		//arrange
		var passwordService = App.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerators.GenerateValidPassword();
		var user = UserGenerators.User
			.Clone()
			.WithPassword(passwordService.HashPassword(rawPassword))
			.WithStatus(UserState.NotActivated)
			.Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync();
		});

		var request = new LoginRequest
		{
			Email = user.Email,
			Password = rawPassword
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.Should().Be401Unauthorized();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(AuthenticationErrors.NotActivatedAccount);
	}

	[Fact]
	public async Task Login_AsUnExistingUser_Should_ResultInUnauthorized()
	{
		//arrange

		var request = new LoginRequest
		{
			Email = F.Internet.Email(),
			Password = UserGenerators.GenerateValidPassword()
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.Should().Be401Unauthorized();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(AuthenticationErrors.InvalidCredentials);
	}

	[Fact]
	public async Task Login_WithIncorrectPassword_Should_ResultInUnauthorized()
	{
		//arrange
		var passwordService = App.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerators.GenerateValidPassword();
		var user = UserGenerators.User
			.Clone()
			.WithPassword(passwordService.HashPassword(rawPassword))
			.Generate();

		await UseDbContextAsync<UserAccessDbContext>(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync();
		});

		var request = new LoginRequest
		{
			Email = user.Email,
			Password = rawPassword + "x"
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.Should().Be401Unauthorized();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(AuthenticationErrors.InvalidCredentials);
	}
}
