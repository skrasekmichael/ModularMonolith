using System.ComponentModel.DataAnnotations;

namespace TeamUp.Common.Infrastructure.Options;

public sealed class JwtOptions : IAppOptions
{
	public static string SectionName => "JwtSettings";

	[Required]
	public required string Issuer { get; init; }

	[Required]
	public required string Audience { get; init; }

	[Required]
	public required string SigningKey { get; init; }

	[Required]
	public required int ExpirationMinutes { get; init; }
}
