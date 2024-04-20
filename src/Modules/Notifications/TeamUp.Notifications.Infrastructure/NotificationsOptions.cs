using System.ComponentModel.DataAnnotations;

using TeamUp.Common.Infrastructure.Options;

namespace TeamUp.Notifications.Infrastructure;

internal sealed class NotificationsOptions : IAppOptions
{
	public static string SectionName => "Modules:Notifications";

	[Required]
	public required MailOptions Mail { get; init; }

	public class MailOptions
	{
		[Required]
		public required string Server { get; init; }

		[Required]
		public required int Port { get; init; }

		[Required]
		public required string SenderName { get; init; }

		[Required, EmailAddress]
		public required string SenderEmail { get; init; }

		[Required]
		public required string UserName { get; init; }

		[Required]
		public required string Password { get; init; }
	}
}
