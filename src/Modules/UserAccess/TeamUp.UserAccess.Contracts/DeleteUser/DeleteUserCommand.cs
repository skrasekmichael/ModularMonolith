using FluentValidation;

using TeamUp.Common.Contracts;

namespace TeamUp.UserAccess.Contracts.DeleteUser;

public sealed record DeleteUserCommand : ICommand
{
	public required UserId InitiatorId { get; init; }
	public required string Password { get; init; }

	public sealed class Validator : AbstractValidator<DeleteUserCommand>
	{
		public Validator()
		{
			RuleFor(x => x.InitiatorId).NotEmpty();
		}
	}
}
