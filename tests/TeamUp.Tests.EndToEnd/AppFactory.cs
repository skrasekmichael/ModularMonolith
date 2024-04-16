using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Services;

namespace TeamUp.Tests.EndToEnd;

[CollectionDefinition(nameof(AppCollectionFixture))]
public sealed class AppCollectionFixture : ICollectionFixture<AppFixture>;

public sealed class AppFactory(string connectionString) : WebApplicationFactory<Program>, IAppFactory<AppFactory>
{
	public static string HttpsPort => "8181";

	public static AppFactory Create(string connectionString) => new(connectionString);

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices(services =>
		{
			//db context
			services.Replace<IDbContextConfigurator, ContainerDbContextConfigurator>(_ => new(connectionString));

			//date time provider
			services.Replace<IDateTimeProvider, SkewDateTimeProvider>();
		});

		builder.UseEnvironment(Environments.Production);
		builder.UseSetting("https_port", HttpsPort);
	}
}
