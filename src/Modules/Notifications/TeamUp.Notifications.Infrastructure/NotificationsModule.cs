using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using TeamUp.Common.Infrastructure;
using TeamUp.Common.Infrastructure.Modules;
using TeamUp.Notifications.Application.Email;
using TeamUp.Notifications.Contracts;
using TeamUp.Notifications.Infrastructure.Persistence;
using TeamUp.Notifications.Infrastructure.Services;

namespace TeamUp.Notifications.Infrastructure;

public sealed class NotificationsModule : Module<NotificationsModuleId, NotificationsDbContext>
{
	public override Assembly ContractsAssembly => typeof(NotificationsModuleId).Assembly;

	public override Assembly ApplicationAssembly => typeof(IEmailSender).Assembly;

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
