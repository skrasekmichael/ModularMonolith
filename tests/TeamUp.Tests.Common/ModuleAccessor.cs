using TeamUp.Common.Infrastructure.Modules;
using TeamUp.Notifications.Infrastructure;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.Tests.Common.Extensions;
using TeamUp.UserAccess.Infrastructure;

namespace TeamUp.Tests.Common;

public static class ModulesAccessor
{
	public static IModule[] Modules { get; } = [
		new UserAccessModule(),
		new TeamManagementModule(),
		new NotificationsModule()
	];

	public static IEnumerable<(Type DbContextType, Type ModuleIdType)> GetModuleParams(this IEnumerable<IModule> modules)
	{
		return modules
			.Select(module => module
				.GetType()
				.BaseType!
				.GetGenericArguments()
				.Map(args => (args[1], args[0])));
	}
}
