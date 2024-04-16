using System.ComponentModel.DataAnnotations;

namespace TeamUp.Common.Infrastructure.Options;

public sealed class ClientOptions : IAppOptions
{
	public static string SectionName => "Client";

	[Required]
	public required string Url { get; init; }
}
