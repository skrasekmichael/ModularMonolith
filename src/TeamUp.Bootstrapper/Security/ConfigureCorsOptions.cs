using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

using TeamUp.Common.Infrastructure.Options;

namespace TeamUp.Bootstrapper.Security;

internal sealed class ConfigureCorsOptions : IConfigureNamedOptions<CorsOptions>
{
	private readonly IOptions<ClientOptions> _clientSettings;

	public ConfigureCorsOptions(IOptions<ClientOptions> clientSettings)
	{
		_clientSettings = clientSettings;
	}

	public void Configure(string? name, CorsOptions options) => Configure(options);

	public void Configure(CorsOptions options)
	{
		options.AddDefaultPolicy(policy =>
		{
			policy.WithOrigins(_clientSettings.Value.Url)
				.AllowAnyHeader()
				.WithMethods("GET", "POST", "PUT", "DELETE");
		});
	}
}
