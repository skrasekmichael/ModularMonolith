using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure;
using TeamUp.Common.Infrastructure.Options;
using TeamUp.Common.Infrastructure.Processing.Inbox;
using TeamUp.Common.Infrastructure.Services;
using TeamUp.Notifications.Application.Email;

namespace TeamUp.Tests.EndToEnd;

[CollectionDefinition(nameof(AppCollectionFixture))]
public sealed class AppCollectionFixture : ICollectionFixture<AppFixture>;

public sealed class AppFactory(string dbConnectionString, string busConnectionString) : WebApplicationFactory<Program>, IAppFactory<AppFactory>
{
	public static string HttpsPort => "8181";

	public static AppFactory Create(string connectionString, string busConnectionString) => new(connectionString, busConnectionString);

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices(services =>
		{
			//db context configuration
			services.Replace<IDbContextConfigurator, ContainerDbContextConfigurator>(_ => new(dbConnectionString));

			//bus configuration
			services.RemoveAll<IOptions<RabbitMqOptions>>();
			services.AddAppOptions<RabbitMqOptions>(options =>
			{
				options.ConnectionString = busConnectionString;
			});

			//date time provider
			services.Replace<IDateTimeProvider, SkewDateTimeProvider>();

			//email service
			services.AddSingleton<MailInbox>();
			services.Replace<IEmailSender, EmailSender>();

			//callback
			services.AddTransient<CallbackCounter>();

			//integration events
			services.AddSingleton(typeof(Owner<,>));
			services.AddScoped<InboxConsumer>();
			services.Replace<IInboxConsumer, InboxConsumerWithCallbacksFacade>();
		});

		builder.UseEnvironment(Environments.Production);
		builder.UseSetting("https_port", HttpsPort);
	}
}
