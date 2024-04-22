using FluentValidation;

using TeamUp.Common.Contracts;

namespace TeamUp.UserAccess.Contracts.Activation;

public sealed record ActivateAccountCommand : ICommand
{
	public required UserId UserId { get; init; }

	public sealed class Validator : AbstractValidator<ActivateAccountCommand>;
}
