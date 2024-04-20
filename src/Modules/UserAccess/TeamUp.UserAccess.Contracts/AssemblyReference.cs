using TeamUp.Common.Contracts;

namespace TeamUp.UserAccess.Contracts;

public sealed class UserAccessModuleId : IModuleId
{
	public static string ModuleName { get; } = "UserAccess";
}
