using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Infrastructure.Modules;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Domain.Aggregates;
using TeamUp.TeamManagement.Endpoints;
using TeamUp.TeamManagement.Infrastructure.Persistence.Users;

namespace TeamUp.TeamManagement.Infrastructure;

public sealed class TeamManagementModule : ModuleWithEndpoints<TeamManagementModuleId, TeamManagementDbContext, TeamManagementEndpointGroup>
{
	public override Assembly ContractsAssembly { get; } = typeof(TeamManagementModuleId).Assembly;
	public override Assembly ApplicationAssembly { get; } = typeof(Application.AssemblyReference).Assembly;

	public override Assembly[] Assemblies => [
		ContractsAssembly,
		ApplicationAssembly,
		typeof(IUserRepository).Assembly,
		typeof(TeamManagementModule).Assembly,
		typeof(TeamManagementEndpointGroup).Assembly
	];

	public override void ConfigureServices(IServiceCollection services)
	{
		services.AddScoped<IUserRepository, UserRepository>();
	}
}
