using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Infrastructure.Modules;

namespace TeamUp.Common.Infrastructure.Extensions;

public static class ModulesExtensions
{
	public static async Task MigrateAsync(this IEnumerable<IModule> modules, IServiceProvider services, CancellationToken ct = default)
	{
		await using var scope = services.CreateAsyncScope();
		var tasks = modules.Select(async module =>
		{
			using var context = module.GetDatabaseContext(scope);
			await context.Database.MigrateAsync(ct);
		});

		await Task.WhenAll(tasks);
	}
}
