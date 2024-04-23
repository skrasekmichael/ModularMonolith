using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

using Bogus;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.UserAccess.Application.Abstractions;
using TeamUp.UserAccess.Domain;

namespace TeamUp.Tests.EndToEnd.EndpointTests;

[Collection(nameof(AppCollectionFixture))]
public abstract class BaseEndpointTests(AppFixture app) : IAsyncLifetime
{
	private static readonly Type IntegrationEventHandlerType = typeof(IIntegrationEventHandler<>);

	protected static Faker F => FakerExtensions.F;

	protected AppFixture App { get; } = app;
	protected HttpClient Client { get; private set; } = null!;

	internal SkewDateTimeProvider DateTimeProvider { get; private set; } = null!;
	internal MailInbox Inbox { get; private set; } = null!;
	internal DelayedCommitUnitOfWorkOptions DelayedCommitUnitOfWorkOptions { get; private set; } = null!;

	public async Task InitializeAsync()
	{
		await App.CleanUpAsync();

		Client = App.CreateClient();
		Client.BaseAddress = new Uri($"https://{Client.BaseAddress!.Host}:{App.HttpsPort}");

		DelayedCommitUnitOfWorkOptions = App.Services.GetRequiredService<DelayedCommitUnitOfWorkOptions>();
		DateTimeProvider = (SkewDateTimeProvider)App.Services.GetRequiredService<IDateTimeProvider>();
		Inbox = App.Services.GetRequiredService<MailInbox>();

		DelayedCommitUnitOfWorkOptions.IsDelayRequested = false;

		DateTimeProvider.Skew = TimeSpan.Zero;
		DateTimeProvider.ExactTime = null;

		Inbox.Clear();
		ClearAllCallbackCounters();
	}

	private void ClearAllCallbackCounters()
	{
		static bool IsIntegrationEventHandlerType(Type type)
		{
			return type
				.GetInterfaces()
				.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == IntegrationEventHandlerType);
		}

		var types = ModulesAccessor.Modules
			.SelectMany(module => module.ApplicationAssembly
				.GetTypes()
				.Where(type => type.IsClass && !type.IsAbstract && IsIntegrationEventHandlerType(type)));

		foreach (var handlerType in types)
		{
			var callbackType = typeof(Owner<,>).MakeGenericType(handlerType, typeof(CallbackCounter));
			var callback = Unsafe.As<Owner<object, CallbackCounter>>(App.Services.GetRequiredService(callbackType));
			callback.Service.Reset();
		}
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

	protected async ValueTask WaitForIntegrationEventHandlerAsync<THandler>(int minInvokes = 1, int millisecondsTimeout = 10_000) where THandler : IIntegrationEventHandlerMarker
	{
		var callback = App.Services.GetRequiredService<Owner<THandler, CallbackCounter>>().Service;
		if (callback.Count >= minInvokes)
		{
			return;
		}

		var waitTask = callback.WaitForCallbackAsync();
		var completedTask = await Task.WhenAny(waitTask, Task.Delay(millisecondsTimeout));
		ReferenceEquals(completedTask, waitTask).Should().BeTrue($"{typeof(THandler).Name} has to be called");
	}

	protected async Task<(HttpResponseMessage A, HttpResponseMessage B)> RunConcurrentRequestsAsync<TModuleId>(Func<Task<HttpResponseMessage>> requestA, Func<Task<HttpResponseMessage>> requestB) where TModuleId : IModuleId
	{
		var beforeCommitCallback = App.Services.GetRequiredService<Owner<TModuleId, IBeforeCommit, CallbackWithTimeout>>().Service;
		var canCommitCallback = App.Services.GetRequiredService<Owner<TModuleId, ICanCommit, CallbackWithTimeout>>().Service;

		var reqA = Task.Run(async () =>
		{
			await beforeCommitCallback.WaitForCallbackAsync();
			DelayedCommitUnitOfWorkOptions.IsDelayRequested = false;
			var response = await requestA();
			canCommitCallback.Invoke();
			return response;
		});

		var reqB = Task.Run(() =>
		{
			DelayedCommitUnitOfWorkOptions.IsDelayRequested = true;
			return requestB();
		});

		return await Task.WhenAll(reqA, reqB).ContinueWith(task => (task.Result[0], task.Result[1]));
	}

	public Task DisposeAsync()
	{
		Client.Dispose();
		return Task.CompletedTask;
	}
}
