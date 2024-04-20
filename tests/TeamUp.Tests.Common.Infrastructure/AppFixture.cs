using Bogus;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Npgsql;

using Respawn;
using Respawn.Graph;

using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

using Xunit;

namespace TeamUp.Tests.Common;

public sealed class AppFixture<TAppFactory> : IAsyncLifetime where TAppFactory : WebApplicationFactory<Program>, IAppFactory<TAppFactory>
{
	private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
		.WithImage("postgres:16.2")
		.WithDatabase("POSTGRES")
		.WithUsername("POSTGRES")
		.WithPassword("DEVPASS")
		.WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
		.WithCleanUp(true)
		.WithAutoRemove(true)
		.Build();

	private readonly RabbitMqContainer _busContainer = new RabbitMqBuilder()
		.WithImage("rabbitmq:3.13.1")
		.WithHostname("rabbitmq")
		.WithUsername("guest")
		.WithPassword("guest")
		.WithCleanUp(true)
		.WithAutoRemove(true)
		.Build();

	private TAppFactory AppFactory { get; set; } = null!;
	private Respawner Respawner { get; set; } = null!;

	public string HttpsPort => TAppFactory.HttpsPort;
	public string ConnectionString => _dbContainer.GetConnectionString() + ";Include Error Detail=true";

	public IServiceProvider Services => AppFactory.Services;

	public Task InitializeAsync()
	{
		Randomizer.Seed = new Random(420_069);
		Faker.DefaultStrictMode = true;

		return Task.WhenAll(InitBusAsync(), InitDatabaseAsync());
	}

	private Task InitBusAsync() => _busContainer.StartAsync();

	private async Task InitDatabaseAsync()
	{
		await _dbContainer.StartAsync();

		var dbModules = ModulesAccessor.Modules.ToList();

		var dbContexts = dbModules.Select(module => module.CreateDatabaseContext(ConnectionString));

		var migrationTasks = dbContexts.Select(async dbContext =>
		{
			await using var transaction = dbContext.Database.BeginTransaction();
			await dbContext.Database.MigrateAsync();
			await transaction.CommitAsync();
		});

		await Task.WhenAll(migrationTasks);

		await using var connection = new NpgsqlConnection(ConnectionString);
		await connection.OpenAsync();

		var migrationTables = dbModules
			.Select(module => module.GetMigrationTable())
			.Select(table => new Table(table.Name, table.Schema))
			.ToArray();

		Respawner = await Respawner.CreateAsync(connection, new()
		{
			DbAdapter = DbAdapter.Postgres,
			TablesToIgnore = migrationTables
		});

		AppFactory = TAppFactory.Create(ConnectionString, _busContainer.GetConnectionString());
	}

	public async Task DisposeAsync()
	{
		await AppFactory.DisposeAsync();
		await Task.WhenAll(DisposeContainer(_dbContainer), DisposeContainer(_busContainer));
	}

	private async Task DisposeContainer<TContainer>(TContainer container) where TContainer : DockerContainer
	{
		await container.StopAsync();
		await container.DisposeAsync();
	}

	public HttpClient CreateClient() => AppFactory.CreateClient();
	public HttpClient CreateClient(WebApplicationFactoryClientOptions options) => AppFactory.CreateClient(options);

	public async Task ResetDatabaseAsync()
	{
		await using var connection = new NpgsqlConnection(ConnectionString);
		await connection.OpenAsync();
		await Respawner.ResetAsync(connection);
	}

	public Task CleanUpAsync() => ResetDatabaseAsync();
}
