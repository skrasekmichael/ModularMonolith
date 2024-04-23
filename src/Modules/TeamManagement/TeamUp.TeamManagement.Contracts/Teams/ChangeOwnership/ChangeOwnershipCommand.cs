using FluentValidation;

using TeamUp.Common.Contracts;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Teams.ChangeOwnership;

public sealed record ChangeOwnershipCommand : ICommand
{
	public required UserId InitiatorId { get; init; }
	public required TeamId TeamId { get; init; }
	public required TeamMemberId NewOwnerId { get; init; }

	public sealed class Validator : AbstractValidator<ChangeOwnershipCommand>
	{
		public Validator()
		{
			RuleFor(x => x.InitiatorId).NotEmpty();
			RuleFor(x => x.TeamId).NotEmpty();
			RuleFor(x => x.NewOwnerId).NotEmpty();
		}
	}
}
