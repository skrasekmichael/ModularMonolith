using FluentValidation;

using TeamUp.Common.Contracts;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Teams.GetUserTeams;

public sealed record GetUserTeamsQuery : IQuery<Collection<TeamSlimResponse>>
{
	public required UserId InitiatorId { get; init; }

	public sealed class Validator : AbstractValidator<GetUserTeamsQuery>
	{
		public Validator()
		{
			RuleFor(x => x.InitiatorId).NotEmpty();
		}
	}
}
