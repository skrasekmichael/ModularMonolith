using RailwayResult;

namespace TeamUp.Notifications.Application.Email;

public interface IEmailSender
{
	public Task<Result> SendEmailAsync(string email, string subject, string message, CancellationToken ct = default);
}
