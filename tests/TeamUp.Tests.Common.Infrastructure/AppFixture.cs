using Bogus;

using DotNet.Testcontainers.Builders;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Npgsql;

using Respawn;
using Respawn.Graph;

using TeamUp.Common.Infrastructure.Modules;
using TeamUp.Common.Infrastructure.Persistence;

using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

using Xunit;

namespace TeamUp.Tests.Common;

public sealed class AppFixture<TAppFactory> : IAsyncLifetime where TAppFactory : WebApplicationFactory<Program>, IAppFactory<TAppFactory>
{
	private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
		.WithDatabase("POSTGRES")
		.WithUsername("POSTGRES")
		.WithPassword("DEVPASS")
		.WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
		.WithCleanUp(true)
		.WithAutoRemove(true)
		.Build();

	private readonly RabbitMqContainer _busContainer = new RabbitMqBuilder()
		.WithHostname("rabbitmq")
		.WithUsername("guest")
		.WithPassword("guest")
		.WithExposedPort(5672)
		.WithPortBinding(5672)
		.WithCleanUp(true)
		.WithAutoRemove(true)
		.Build();

	private TAppFactory AppFactory { get; set; } = null!;
	private Respawner Respawner { get; set; } = null!;

	public string HttpsPort => TAppFactory.HttpsPort;
	public string ConnectionString => _dbContainer.GetConnectionString();

	public IServiceProvider Services => AppFactory.Services;

	public AppFixture()
	{
		Randomizer.Seed = new Random(420_069);
		Faker.DefaultStrictMode = true;
	}

	public Task InitializeAsync() => Task.WhenAll(InitBusAsync(), InitDatabaseAsync());

	private Task InitBusAsync() => _busContainer.StartAsync();

	private async Task InitDatabaseAsync()
	{
		await _dbContainer.StartAsync();

		var dbModules = ModulesAccessor.Modules.OfType<IModuleWithDatabase>().ToList();

		var dbContexts = new List<DbContext>(dbModules.Count + 1)
		{
			DatabaseUtils.CreateDatabaseContext<OutboxDbContext>(ConnectionString)
		};

		dbContexts.AddRange(dbModules.Select(module => module.CreateDatabaseContext(ConnectionString)));

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
			.Append(DatabaseUtils.GetMigrationsTable<OutboxDbContext>())
			.Select(table => new Table(table.Name, table.Schema))
			.ToArray();

		Respawner = await Respawner.CreateAsync(connection, new()
		{
			DbAdapter = DbAdapter.Postgres,
			TablesToIgnore = migrationTables
		});

		AppFactory = TAppFactory.Create(ConnectionString);
	}

	public Task DisposeAsync() => Task.WhenAll(
		AppFactory.DisposeAsync().AsTask(),
		_dbContainer.DisposeAsync().AsTask(),
		_busContainer.DisposeAsync().AsTask()
	);

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
