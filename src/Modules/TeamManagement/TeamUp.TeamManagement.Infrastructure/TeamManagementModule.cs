using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Infrastructure.Modules;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.TeamManagement.Infrastructure.Persistence.Users;

namespace TeamUp.TeamManagement.Infrastructure;

public sealed class TeamManagementModule : ModuleWithDatabase<TeamManagementDbContext>
{
	public override Assembly ContractsAssembly { get; } = typeof(Contracts.AssemblyReference).Assembly;
	public override Assembly ApplicationAssembly { get; } = typeof(Application.AssemblyReference).Assembly;

	public override Assembly[] Assemblies => [
		ContractsAssembly,
		ApplicationAssembly,
		typeof(Domain.AssemblyReference).Assembly,
		typeof(TeamManagementModule).Assembly,
		typeof(Endpoints.AssemblyReference).Assembly
	];

	public override void ConfigureServices(IServiceCollection services)
	{
		services.AddScoped<IUserRepository, UserRepository>();
	}
}
