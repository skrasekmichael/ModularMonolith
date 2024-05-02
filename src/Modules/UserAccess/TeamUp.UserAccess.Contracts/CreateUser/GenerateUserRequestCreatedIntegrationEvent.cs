using FluentValidation;

using TeamUp.Common.Contracts;

namespace TeamUp.UserAccess.Contracts.CreateUser;

public sealed record GenerateUserRequestCreatedIntegrationEvent : IIntegrationEvent
{
	public required string Name { get; init; }
	public required string Email { get; init; }

	public sealed class Validator : AbstractValidator<GenerateUserRequestCreatedIntegrationEvent>
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
