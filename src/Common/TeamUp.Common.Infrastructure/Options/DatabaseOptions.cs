using System.ComponentModel.DataAnnotations;

namespace TeamUp.Common.Infrastructure.Options;

public sealed class DatabaseOptions : IAppOptions
{
	public static string SectionName => "Database";

	[Required]
	public required string ConnectionString { get; init; }
}
