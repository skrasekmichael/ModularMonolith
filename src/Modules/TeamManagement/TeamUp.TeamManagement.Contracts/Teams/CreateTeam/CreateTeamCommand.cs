using FluentValidation;

using TeamUp.Common.Contracts;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Teams.CreateTeam;

public sealed class CreateTeamCommand : ICommand<TeamId>
{
	public required UserId OwnerId { get; init; }
	public required string Name { get; init; }

	public sealed class Validator : AbstractValidator<CreateTeamCommand>
	{
		public Validator()
		{
			RuleFor(x => x.OwnerId).NotEmpty();
			RuleFor(x => x.Name)
				.NotEmpty()
				.MinimumLength(TeamConstants.TEAM_NAME_MIN_SIZE)
				.MaximumLength(TeamConstants.TEAM_NAME_MAX_SIZE);
		}
	}
}
