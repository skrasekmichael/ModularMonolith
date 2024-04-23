using FluentValidation;

using TeamUp.Common.Contracts;
using TeamUp.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Teams.GetTeam;

public sealed record GetTeamQuery : IQuery<TeamResponse>
{
	public required UserId InitiatorId { get; init; }
	public required TeamId TeamId { get; init; }

	public sealed class Validator : AbstractValidator<GetTeamQuery>
	{
		public Validator()
		{
			RuleFor(x => x.InitiatorId).NotEmpty();
			RuleFor(x => x.TeamId).NotEmpty();
		}
	}
}
