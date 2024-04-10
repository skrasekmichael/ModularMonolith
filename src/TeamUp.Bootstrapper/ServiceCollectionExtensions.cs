using Asp.Versioning;

using Microsoft.OpenApi.Models;

using TeamUp.Common.Infrastructure;

namespace TeamUp.Bootstrapper;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddVersioning(this IServiceCollection services)
	{
		services.AddApiVersioning(options =>
		{
			options.DefaultApiVersion = new ApiVersion(1);
			options.ReportApiVersions = true;
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.ApiVersionReader = ApiVersionReader.Combine(
				new UrlSegmentApiVersionReader(),
				new HeaderApiVersionReader("X-Api-Version")
			);
		}).AddApiExplorer(options =>
		{
			options.GroupNameFormat = "'v'V";
			options.SubstituteApiVersionInUrl = true;
		});

		return services;
	}

	public static IServiceCollection AddSwagger(this IServiceCollection services)
	{
		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo { Title = "TeamUp API", Version = "v1" });
			options.AddSecurityDefinition("Bearer", new()
			{
				In = ParameterLocation.Header,
				Description = "Please enter a valid token",
				Name = "Authorization",
				Type = SecuritySchemeType.Http,
				BearerFormat = "JWT",
				Scheme = "Bearer"
			});
			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new()
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					[]
				}
			});
		});

		return services;
	}

	public static IServiceCollection AddModule<TModule>(this IServiceCollection services) where TModule : BaseModule, new()
	{
		var module = new TModule();

		module.ConfigureService(services);
		module.ConfigureHealthChecks(services.AddHealthChecks());

		return services;
	}
}
