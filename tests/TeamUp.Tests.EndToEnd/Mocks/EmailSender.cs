using RailwayResult;

using TeamUp.Notifications.Application;

namespace TeamUp.Tests.EndToEnd.Mocks;

internal sealed class MailInbox : List<(string EmailAddress, string Subject, string Message)>;

internal sealed class EmailSender : IEmailSender
{
	private readonly MailInbox _inbox;

	public EmailSender(MailInbox inbox)
	{
		_inbox = inbox;
	}

	public Task<Result> SendEmailAsync(string email, string subject, string message, CancellationToken ct = default)
	{
		lock (_inbox)
		{
			_inbox.Add((email, subject, message));
		}

		return Task.FromResult(Result.Success);
	}
}
