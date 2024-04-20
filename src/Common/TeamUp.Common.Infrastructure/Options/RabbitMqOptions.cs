using System.ComponentModel.DataAnnotations;

namespace TeamUp.Common.Infrastructure.Options;

internal sealed record RabbitMqOptions : IAppOptions
{
	public static string SectionName => "RabbitMq";

	[Required]
	public required string ConnectionString { get; set; }
}
