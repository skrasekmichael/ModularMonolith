using FluentValidation;

using TeamUp.Common.Contracts;

namespace TeamUp.UserAccess.Contracts.CompleteRegistration;

public sealed record CompleteRegistrationCommand : ICommand
{
	public required UserId UserId { get; init; }
	public required string Password { get; init; }

	public sealed class Validator : AbstractValidator<CompleteRegistrationCommand>
	{
		public Validator()
		{
			RuleFor(x => x.UserId).NotEmpty();
			RuleFor(x => x.Password).NotEmpty();
		}
	}
}
