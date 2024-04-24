using FluentValidation;

using TeamUp.Common.Contracts;

namespace TeamUp.UserAccess.Contracts.CreateUser;

public sealed record GenerateUserCommand : ICommand<UserId>
{
	public required string Name { get; init; }
	public required string Email { get; init; }

	public sealed class Validator : AbstractValidator<GenerateUserCommand>
	{
		public Validator()
		{
			RuleFor(x => x.Name)
				.NotEmpty()
				.MinimumLength(UserConstants.USERNAME_MIN_SIZE)
				.MaximumLength(UserConstants.USERNAME_MAX_SIZE);

			RuleFor(x => x.Email).EmailAddress();
		}
	}
}
