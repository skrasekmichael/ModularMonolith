using FluentValidation;
using TeamUp.UserAccess.Contracts;
using TeamUp.Common.Contracts;

namespace TeamUp.TeamManagement.Contracts.Teams.ChangeNickname;

public sealed record ChangeNicknameCommand : ICommand
{
	public required UserId InitiatorId { get; init; }
	public required TeamId TeamId { get; init; }
	public required string Nickname { get; init; }

	public sealed class Validator : AbstractValidator<ChangeNicknameCommand>
	{
		public Validator()
		{
			RuleFor(x => x.InitiatorId).NotEmpty();
			RuleFor(x => x.TeamId).NotEmpty();
			RuleFor(x => x.Nickname)
				.NotEmpty()
				.MinimumLength(TeamConstants.NICKNAME_MIN_SIZE)
				.MaximumLength(TeamConstants.NICKNAME_MAX_SIZE);
		}
	}
}
