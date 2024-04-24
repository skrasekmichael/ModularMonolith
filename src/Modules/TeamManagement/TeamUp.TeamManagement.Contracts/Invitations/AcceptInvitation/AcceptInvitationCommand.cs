using FluentValidation;

using TeamUp.Common.Contracts;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Invitations.AcceptInvitation;

public sealed record AcceptInvitationCommand : ICommand
{
	public required UserId InitiatorId { get; init; }
	public required InvitationId InvitationId { get; init; }

	public sealed class Validator : AbstractValidator<AcceptInvitationCommand>
	{
		public Validator()
		{
			RuleFor(x => x.InitiatorId).NotEmpty();
			RuleFor(x => x.InvitationId).NotEmpty();
		}
	}
}
