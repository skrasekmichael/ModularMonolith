﻿using RailwayResult;

using TeamUp.Common.Application;
using TeamUp.Notifications.Contracts;

namespace TeamUp.Notifications.Application.Email;

internal sealed class SendEmailCommandHandler : ICommandHandler<SendEmailCommand>
{
	private readonly IEmailSender _emailSender;

	public SendEmailCommandHandler(IEmailSender emailSender)
	{
		_emailSender = emailSender;
	}

	public Task<Result> Handle(SendEmailCommand command, CancellationToken ct) => _emailSender.SendEmailAsync(command.Email, command.Subject, command.Message, ct);
}
