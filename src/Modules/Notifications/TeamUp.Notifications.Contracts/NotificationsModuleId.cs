using TeamUp.Common.Contracts;

namespace TeamUp.Notifications.Contracts;

public sealed class NotificationsModuleId : IModuleId
{
	public static string ModuleName { get; } = "Notifications";
}
