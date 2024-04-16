using FluentValidation;

using TeamUp.Application.Abstractions;

namespace TeamUp.UserAccess.Contracts.Login;

public sealed record LoginCommand : ICommand<string>
{
	public required string Email { get; init; }
	public required string Password { get; init; }

	public sealed class Validator : AbstractValidator<LoginCommand>
	{
		public Validator()
		{
			RuleFor(x => x.Email)
				.NotEmpty()
				.EmailAddress();

			RuleFor(x => x.Password)
				.NotEmpty();
		}
	}

}
