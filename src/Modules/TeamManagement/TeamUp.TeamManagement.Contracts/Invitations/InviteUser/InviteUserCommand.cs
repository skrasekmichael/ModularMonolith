using FluentValidation;

using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Invitations.InviteUser;

public sealed record InviteUserCommand : ICommand
{
	public required UserId InitiatorId { get; init; }
	public required TeamId TeamId { get; init; }
	public required string Email { get; init; }

	public sealed class Validator : AbstractValidator<InviteUserCommand>
	{
		public Validator()
		{
			RuleFor(x => x.InitiatorId).NotEmpty();
			RuleFor(x => x.TeamId).NotEmpty();
			RuleFor(x => x.Email).EmailAddress();
		}
	}
}
