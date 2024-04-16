using System.Net.Http.Headers;

using Bogus;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.UserAccess.Application.Abstractions;
using TeamUp.UserAccess.Domain;

namespace TeamUp.Tests.EndToEnd.EndpointTests;

[Collection(nameof(AppCollectionFixture))]
public abstract class BaseEndpointTests(AppFixture app) : IAsyncLifetime
{
	protected static Faker F => FakerExtensions.F;

	protected AppFixture App { get; } = app;
	protected HttpClient Client { get; private set; } = null!;
	internal SkewDateTimeProvider DateTimeProvider { get; private set; } = null!;

	public async Task InitializeAsync()
	{
		await App.CleanUpAsync();

		Client = App.CreateClient();
		Client.BaseAddress = new Uri($"https://{Client.BaseAddress!.Host}:{App.HttpsPort}");

		DateTimeProvider = (SkewDateTimeProvider)App.Services.GetRequiredService<IDateTimeProvider>();

		DateTimeProvider.Skew = TimeSpan.Zero;
		DateTimeProvider.ExactTime = null;
	}

	public void Authenticate(User user)
	{
		var tokenService = App.Services.GetRequiredService<ITokenService>();
		var jwt = tokenService.GenerateToken(user);

		Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
	}

	protected async Task UseDbContextAsync<TDatabaseContext>(Func<TDatabaseContext, Task> apply) where TDatabaseContext : DbContext, IDatabaseContext
	{
		await using var scope = App.Services.CreateAsyncScope();

		await using var dbContext = scope.ServiceProvider.GetRequiredService<TDatabaseContext>();
		await apply(dbContext);

		await scope.DisposeAsync();
	}

	protected async ValueTask<T> UseDbContextAsync<TDatabaseContext, T>(Func<TDatabaseContext, ValueTask<T>> apply) where TDatabaseContext : DbContext, IDatabaseContext
	{
		await using var scope = App.Services.CreateAsyncScope();

		await using var dbContext = scope.ServiceProvider.GetRequiredService<TDatabaseContext>();
		var result = await apply(dbContext);

		await scope.DisposeAsync();
		return result;
	}

	protected async Task<T> UseDbContextAsync<TDatabaseContext, T>(Func<TDatabaseContext, Task<T>> apply) where TDatabaseContext : DbContext, IDatabaseContext
	{
		await using var scope = App.Services.CreateAsyncScope();

		await using var dbContext = scope.ServiceProvider.GetRequiredService<TDatabaseContext>();
		var result = await apply(dbContext);

		await scope.DisposeAsync();
		return result;
	}

	protected Task WaitForIntegrationEventsAsync() => Task.Delay(800);

	public Task DisposeAsync()
	{
		Client.Dispose();
		return Task.CompletedTask;
	}
}
