using FluentValidation;

using TeamUp.Common.Contracts;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Teams.SetTeamName;

public sealed record SetTeamNameCommand : ICommand
{
	public required UserId InitiatorId { get; init; }
	public required TeamId TeamId { get; init; }
	public required string Name { get; init; }

	public sealed class Validator : AbstractValidator<SetTeamNameCommand>
	{
		public Validator()
		{
			RuleFor(x => x.InitiatorId).NotEmpty();
			RuleFor(x => x.TeamId).NotEmpty();
			RuleFor(x => x.Name)
				.NotEmpty()
				.MinimumLength(TeamConstants.TEAM_NAME_MIN_SIZE)
				.MaximumLength(TeamConstants.TEAM_NAME_MAX_SIZE);
		}
	}
}
