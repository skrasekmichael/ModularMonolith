using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Infrastructure;
using TeamUp.Common.Infrastructure.Modules;
using TeamUp.UserAccess.Application.Abstractions;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Domain;
using TeamUp.UserAccess.Endpoints;
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
			.AddScoped<IUserAccessQueryContext, UserAccessDbQueryContextFacade>()
			.AddScoped<IUserRepository, UserRepository>()
			.AddScoped<UserFactory>();
	}
}
