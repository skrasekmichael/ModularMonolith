using RailwayResult;

namespace TeamUp.Notifications.Application;

public interface IEmailSender
{
	public Task<Result> SendEmailAsync(string email, string subject, string message, CancellationToken ct = default);
}
