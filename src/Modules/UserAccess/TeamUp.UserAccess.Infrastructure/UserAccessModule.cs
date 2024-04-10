using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Infrastructure;

namespace TeamUp.UserAccess.Infrastructure;

public sealed class UserAccessModule : BaseModule
{
	public override Assembly[] Assemblies => [
		Assembly.LoadFrom("TeamUp.UserAccess.Application.dll"),
		Assembly.LoadFrom("TeamUp.UserAccess.Domain.dll"),
		Assembly.LoadFrom("TeamUp.UserAccess.Contracts.dll"),
		Assembly.LoadFrom("TeamUp.UserAccess.Infrastructure.dll"),
		Assembly.LoadFrom("TeamUp.UserAccess.Endpoints.dll")
	];

	public override void ConfigureService(IServiceCollection services)
	{

	}
}
