using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Infrastructure;
using TeamUp.Common.Infrastructure.Modules;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.UserAccess.Application.Abstractions;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Domain;
using TeamUp.UserAccess.Infrastructure.Persistence;
using TeamUp.UserAccess.Infrastructure.Services;

namespace TeamUp.UserAccess.Infrastructure;

public sealed class UserAccessModule : ModuleWithDatabase<UserAccessDbContext>
{
	public override Assembly ContractsAssembly { get; } = typeof(Contracts.AssemblyReference).Assembly;
	public override Assembly ApplicationAssembly { get; } = typeof(Application.AssemblyReference).Assembly;

	public override Assembly[] Assemblies => [
		ContractsAssembly,
		ApplicationAssembly,
		typeof(Domain.AssemblyReference).Assembly,
		typeof(UserAccessModule).Assembly,
		typeof(Endpoints.AssemblyReference).Assembly
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
