using System.ComponentModel.DataAnnotations;

namespace TeamUp.Common.Infrastructure.Options;

public sealed class ClientOptions : IAppOptions
{
	public static string SectionName => "Client";

	[Required]
	public required string Url { get; init; }

	[Required]
	public required string ActivateAccountUrl { get; init; }

	[Required]
	public required string CompleteAccountRegistrationUrl { get; init; }
}
