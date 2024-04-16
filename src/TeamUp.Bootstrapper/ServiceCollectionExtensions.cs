using Asp.Versioning;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

using TeamUp.Bootstrapper.Security;

namespace TeamUp.Bootstrapper;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddRestApiVersioning(this IServiceCollection services)
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

	public static IServiceCollection AddSecurity(this IServiceCollection services)
	{
		services.AddCors();
		services.ConfigureOptions<ConfigureCorsOptions>();

		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer();
		services.ConfigureOptions<ConfigureJwtBearerOptions>();

		services.AddAuthorization();

		return services;
	}
}
