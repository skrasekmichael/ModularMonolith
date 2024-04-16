using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using TeamUp.Common.Infrastructure.Options;

namespace TeamUp.Bootstrapper.Security;

internal sealed class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
	private readonly IOptions<JwtOptions> _jwtSettings;

	public ConfigureJwtBearerOptions(IOptions<JwtOptions> jwtSettings)
	{
		_jwtSettings = jwtSettings;
	}

	public void Configure(string? name, JwtBearerOptions options) => Configure(options);

	public void Configure(JwtBearerOptions options)
	{
		var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Value.SigningKey));

		options.TokenValidationParameters = new()
		{
			ClockSkew = TimeSpan.Zero,
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = _jwtSettings.Value.Issuer,
			ValidAudience = _jwtSettings.Value.Audience,
			IssuerSigningKey = signingKey
		};

		options.SaveToken = true;
	}
}
