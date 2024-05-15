using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Quartz;

using TeamUp.Common.Infrastructure;
using TeamUp.Common.Infrastructure.Modules;
using TeamUp.UserAccess.Application.Abstractions;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Domain;
using TeamUp.UserAccess.Endpoints;
using TeamUp.UserAccess.Infrastructure.Jobs;
using TeamUp.UserAccess.Infrastructure.Persistence;
using TeamUp.UserAccess.Infrastructure.Services;

namespace TeamUp.UserAccess.Infrastructure;

public sealed class UserAccessModule : ModuleWithEndpoints<UserAccessModuleId, UserAccessDbContext, UserAccessEndpointGroup>
{
	public override Assembly ContractsAssembly { get; } = typeof(UserAccessModuleId).Assembly;
	public override Assembly ApplicationAssembly { get; } = typeof(IPasswordService).Assembly;

	public override Assembly[] Assemblies => [
		ContractsAssembly,
		ApplicationAssembly,
		typeof(IUserRepository).Assembly,
		typeof(UserAccessModule).Assembly,
		typeof(UserAccessEndpointGroup).Assembly
	];

	public override void ConfigureServices(IServiceCollection services)
	{
		services.AddAppOptions<UserAccessOptions>();

		services
			.AddSingleton<IPasswordService, PasswordService>()
			.AddSingleton<ITokenService, JwtTokenService>()
			.AddSingleton<IClientUrlGenerator, ClientUrlGenerator>()
			.AddScoped<IUserAccessQueryContext, UserAccessDbQueryContextFacade>()
			.AddScoped<IUserRepository, UserRepository>()
			.AddScoped<UserFactory>()
			.AddScoped<ICleanExpiredAccountsJob, CleanExpiredAccountsJob>();
	}

	public override void ConfigureJobs(IServiceCollectionQuartzConfigurator configurator)
	{
		var cleanExpiredAccountsJobKey = new JobKey(nameof(ICleanExpiredAccountsJob));
		configurator
			.AddJob<ICleanExpiredAccountsJob>(cleanExpiredAccountsJobKey)
			.AddTrigger(trigger =>
			{
				trigger
					.ForJob(cleanExpiredAccountsJobKey)
					.WithSimpleSchedule(schedule => schedule
						.WithIntervalInHours(23)
						.RepeatForever());
			});
	}
}
