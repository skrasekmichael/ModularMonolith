using TeamUp.Common.Infrastructure.Modules;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.UserAccess.Infrastructure;

namespace TeamUp.Tests.Common;

public static class ModulesAccessor
{
	public static IModule[] Modules { get; } = [
		new UserAccessModule(),
		new TeamManagementModule()
	];
}
