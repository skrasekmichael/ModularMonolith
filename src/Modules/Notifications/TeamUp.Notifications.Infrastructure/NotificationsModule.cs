using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Infrastructure;
using TeamUp.Common.Infrastructure.Modules;
using TeamUp.Notifications.Application;
using TeamUp.Notifications.Infrastructure.Services;

using Module = TeamUp.Common.Infrastructure.Modules.Module;

namespace TeamUp.Notifications.Infrastructure;

public sealed class NotificationsModule : Module
{
	public override Assembly ContractsAssembly => typeof(Contracts.AssemblyReference).Assembly;

	public override Assembly ApplicationAssembly => typeof(Application.AssemblyReference).Assembly;

	public override Assembly[] Assemblies => [
		ContractsAssembly,
		ApplicationAssembly,
		typeof(NotificationsModule).Assembly
	];

	public override void ConfigureServices(IServiceCollection services)
	{
		services.AddAppOptions<NotificationsOptions>();

		services.AddSingleton<IEmailSender, EmailSender>();
	}
}
