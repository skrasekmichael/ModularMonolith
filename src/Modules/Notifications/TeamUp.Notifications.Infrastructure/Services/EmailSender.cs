using MailKit.Net.Smtp;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MimeKit;

using RailwayResult;
using RailwayResult.Errors;

using TeamUp.Notifications.Application.Email;

namespace TeamUp.Notifications.Infrastructure.Services;

internal sealed class EmailSender : IEmailSender
{
	private static readonly InternalError SendEmailError = new("Notifications.SendEmail.InternalError", "Failed to send email.");

	private readonly IOptions<NotificationsOptions> _options;
	private readonly ILogger<EmailSender> _logger;

	public EmailSender(IOptions<NotificationsOptions> options, ILogger<EmailSender> logger)
	{
		_options = options;
		_logger = logger;
	}

	public async Task<Result> SendEmailAsync(string email, string subject, string message, CancellationToken ct = default)
	{
		var mailOptions = _options.Value.Mail;

		var body = new MimeMessage();
		body.From.Add(new MailboxAddress(mailOptions.SenderName, mailOptions.SenderEmail));
		body.To.Add(new MailboxAddress(string.Empty, email));
		body.Subject = subject;
		body.Body = new TextPart("plain")
		{
			Text = message
		};

		try
		{
			using var client = new SmtpClient();
			client.Connect(mailOptions.Server, mailOptions.Port, false, ct);
			client.Authenticate(mailOptions.UserName, mailOptions.Password, ct);

			var response = await client.SendAsync(body, ct);
			_logger.LogInformation("Mail Server response {response}", response);
			client.Disconnect(true, ct);
			return Result.Success;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send email to {emailAddress}.", email);
			return SendEmailError;
		}
	}
}
