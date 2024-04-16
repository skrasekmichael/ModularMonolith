using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Options;
using TeamUp.UserAccess.Application.Abstractions;
using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Infrastructure.Services;

internal sealed class JwtTokenService : ITokenService
{
	private readonly ILogger<JwtTokenService> _logger;
	private readonly IOptions<JwtOptions> _jwtSettings;
	private readonly IDateTimeProvider _dateTimeProvider;

	public JwtTokenService(ILogger<JwtTokenService> logger, IOptions<JwtOptions> jwtSettings, IDateTimeProvider dateTimeProvider)
	{
		_logger = logger;
		_jwtSettings = jwtSettings;
		_dateTimeProvider = dateTimeProvider;
	}

	public string GenerateToken(User user)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var token = new JwtSecurityToken(
			issuer: _jwtSettings.Value.Issuer,
			audience: _jwtSettings.Value.Audience,
			notBefore: _dateTimeProvider.UtcNow.AddMinutes(0),
			expires: _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.Value.ExpirationMinutes),
			signingCredentials: GetSigningCredentials(),
			claims: GetClaims(user)
		);

		_logger.LogInformation("JWT Created for user {email}", user.Email);
		return tokenHandler.WriteToken(token);
	}

	private List<Claim> GetClaims(User user) => [
		new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
		new(JwtRegisteredClaimNames.Iat, _dateTimeProvider.DateTimeOffsetUtcNow.ToUnixTimeSeconds().ToString()),
		new(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
		new(ClaimTypes.Name, user.Name),
		new(ClaimTypes.Email, user.Email)
	];

	private SigningCredentials GetSigningCredentials()
	{
		var key = Encoding.UTF8.GetBytes(_jwtSettings.Value.SigningKey);
		var symmetricSecurityKey = new SymmetricSecurityKey(key);
		return new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha512);
	}
}
