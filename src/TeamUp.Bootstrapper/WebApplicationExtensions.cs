using Asp.Versioning;

namespace TeamUp.Bootstrapper;

public static class WebApplicationExtensions
{
	public static void MapEndpoints(this WebApplication app, Action<RouteGroupBuilder> mapper)
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

		mapper(apiGroup);
	}
}
