
using FluentValidation;

using TeamUp.Application.Abstractions;

namespace TeamUp.Notifications.Contracts;

public sealed record SendEmailCommand : ICommand
{
	public required string Email { get; init; }
	public required string Subject { get; init; }
	public required string Message { get; init; }

	public sealed class Validator : AbstractValidator<SendEmailCommand>
	{
		public Validator()
		{
			RuleFor(x => x.Email)
				.NotEmpty()
				.EmailAddress();

			RuleFor(x => x.Subject)
				.NotEmpty();

			RuleFor(x => x.Message)
				.NotEmpty();
		}
	}
}
