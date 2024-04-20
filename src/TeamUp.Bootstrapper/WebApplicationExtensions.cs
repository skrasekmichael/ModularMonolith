using Asp.Versioning;

using TeamUp.Common.Infrastructure.Modules;

namespace TeamUp.Bootstrapper;

public static class WebApplicationExtensions
{
	public static void MapEndpoints(this WebApplication app, IEnumerable<IModule> modules)
	{
		var apiVersionSet = app
			.NewApiVersionSet()
			.HasApiVersion(new ApiVersion(1))
			.ReportApiVersions()
			.Build();

		var apiGroup = app
			.MapGroup("api/v{version:apiVersion}")
			.WithApiVersionSet(apiVersionSet)
			.WithOpenApi();

		foreach (var module in modules.OfType<IModuleWithEndpoints>())
		{
			module.MapEndpoints(apiGroup);
		}
	}
}
