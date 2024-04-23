using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Infrastructure.Modules;
using TeamUp.TeamManagement.Application;
using TeamUp.TeamManagement.Contracts;
using TeamUp.TeamManagement.Domain.Aggregates.Events;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Users;
using TeamUp.TeamManagement.Endpoints;
using TeamUp.TeamManagement.Infrastructure.Persistence.Domain.Events;
using TeamUp.TeamManagement.Infrastructure.Persistence.Domain.Invitations;
using TeamUp.TeamManagement.Infrastructure.Persistence.Domain.Teams;
using TeamUp.TeamManagement.Infrastructure.Persistence.Domain.Users;
using TeamUp.TeamManagement.Infrastructure.Services;

namespace TeamUp.TeamManagement.Infrastructure;

public sealed class TeamManagementModule : ModuleWithEndpoints<TeamManagementModuleId, TeamManagementDbContext, TeamManagementEndpointGroup>
{
	public override Assembly ContractsAssembly { get; } = typeof(TeamManagementModuleId).Assembly;
	public override Assembly ApplicationAssembly { get; } = typeof(ITeamManagementQueryContext).Assembly;

	public override Assembly[] Assemblies => [
		ContractsAssembly,
		ApplicationAssembly,
		typeof(IUserRepository).Assembly,
		typeof(TeamManagementModule).Assembly,
		typeof(TeamManagementEndpointGroup).Assembly
	];

	public override void ConfigureServices(IServiceCollection services)
	{
		services
			.AddScoped<IUserRepository, UserRepository>()
			.AddScoped<ITeamRepository, TeamRepository>()
			.AddScoped<IEventRepository, EventRepository>()
			.AddScoped<IInvitationRepository, InvitationRepository>()
			.AddScoped<InvitationFactory>()
			.AddScoped<ITeamManagementQueryContext, TeamManagementDbQueryContextFacade>();
	}
}
