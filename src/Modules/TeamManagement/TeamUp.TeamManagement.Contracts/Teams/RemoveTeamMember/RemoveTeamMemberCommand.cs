using FluentValidation;
using TeamUp.UserAccess.Contracts;
using TeamUp.Common.Contracts;

namespace TeamUp.TeamManagement.Contracts.Teams.RemoveTeamMember;

public sealed record RemoveTeamMemberCommand : ICommand
{
	public required UserId InitiatorId { get; init; }
	public required TeamId TeamId { get; init; }
	public required TeamMemberId MemberId { get; init; }

	public sealed class Validator : AbstractValidator<RemoveTeamMemberCommand>
	{
		public Validator()
		{
			RuleFor(x => x.InitiatorId).NotEmpty();
			RuleFor(x => x.TeamId).NotEmpty();
			RuleFor(x => x.MemberId).NotEmpty();
		}
	}
}
