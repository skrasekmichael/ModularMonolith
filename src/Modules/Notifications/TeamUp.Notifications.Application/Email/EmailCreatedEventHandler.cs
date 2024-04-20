using RailwayResult;

using TeamUp.Common.Application;
using TeamUp.Notifications.Contracts;

namespace TeamUp.Notifications.Application.Email;

internal sealed class EmailCreatedEventHandler : IIntegrationEventHandler<EmailCreatedIntegrationEvent>
{
	private readonly IEmailSender _emailSender;

	public EmailCreatedEventHandler(IEmailSender emailSender)
	{
		_emailSender = emailSender;
	}

	public Task<Result> Handle(EmailCreatedIntegrationEvent command, CancellationToken ct) => _emailSender.SendEmailAsync(command.Email, command.Subject, command.Message, ct);
}
