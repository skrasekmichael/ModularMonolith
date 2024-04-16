using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Infrastructure.Modules;
using TeamUp.Common.Infrastructure.Persistence;

namespace TeamUp.Common.Infrastructure.Extensions;

public static class ModulesExtensions
{
	public static async Task MigrateAsync(this IEnumerable<IModule> modules, IServiceProvider services, CancellationToken ct = default)
	{
		var dbModules = modules.OfType<IModuleWithDatabase>().ToList();

		await using var scope = services.CreateAsyncScope();

		var dbContexts = new List<DbContext>(dbModules.Count + 1)
		{
			scope.ServiceProvider.GetRequiredService<OutboxDbContext>()
		};

		dbContexts.AddRange(dbModules.Select(module => module.GetDatabaseContext(scope)));

		await Task.WhenAll(dbContexts.Select(async context =>
		{
			await context.Database.MigrateAsync(ct);
			await context.DisposeAsync();
		}));
	}
}
