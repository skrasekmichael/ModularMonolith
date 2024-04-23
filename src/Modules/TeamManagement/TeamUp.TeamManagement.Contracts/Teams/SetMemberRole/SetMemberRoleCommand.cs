using FluentValidation;

using TeamUp.Common.Contracts;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Teams.SetMemberRole;

public sealed record SetMemberRoleCommand : ICommand
{
	public required UserId InitiatorId { get; init; }
	public required TeamId TeamId { get; init; }
	public required TeamMemberId MemberId { get; init; }
	public required TeamRole Role { get; init; }

	public sealed class Validator : AbstractValidator<SetMemberRoleCommand>
	{
		public Validator()
		{
			RuleFor(x => x.InitiatorId).NotEmpty();
			RuleFor(x => x.TeamId).NotEmpty();
			RuleFor(x => x.MemberId).NotEmpty();
			RuleFor(x => x.Role).IsInEnum();
		}
	}
}
