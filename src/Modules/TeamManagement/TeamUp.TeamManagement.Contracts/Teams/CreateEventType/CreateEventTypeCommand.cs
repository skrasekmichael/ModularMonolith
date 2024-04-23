using FluentValidation;

using TeamUp.Common.Contracts;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Teams.CreateEventType;

public sealed record CreateEventTypeCommand : ICommand<EventTypeId>
{
	public required UserId InitiatorId { get; init; }
	public required TeamId TeamId { get; init; }
	public required string Name { get; init; }
	public required string Description { get; init; }

	public sealed class Validator : AbstractValidator<CreateEventTypeCommand>
	{
		public Validator()
		{
			RuleFor(x => x.InitiatorId).NotEmpty();
			RuleFor(x => x.TeamId).NotEmpty();

			RuleFor(x => x.Name)
				.NotEmpty()
				.MinimumLength(TeamConstants.EVENTTYPE_NAME_MIN_SIZE)
				.MaximumLength(TeamConstants.EVENTTYPE_NAME_MAX_SIZE);

			RuleFor(x => x.Description)
				.MaximumLength(TeamConstants.EVENTTYPE_DESCRIPTION_MAX_SIZE);
		}
	}
}
