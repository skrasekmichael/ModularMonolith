using FluentValidation;

using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Events.CreateEvent;

public sealed record CreateEventCommand : ICommand<EventId>
{
	public required UserId InitiatorId { get; init; }
	public required TeamId TeamId { get; init; }
	public required EventTypeId EventTypeId { get; init; }
	public required DateTime FromUtc { get; init; }
	public required DateTime ToUtc { get; init; }
	public required string Description { get; init; }
	public required TimeSpan MeetTime { get; init; }
	public required TimeSpan ReplyClosingTimeBeforeMeetTime { get; init; }

	public sealed class Validator : AbstractValidator<CreateEventCommand>
	{
		public Validator(IDateTimeProvider dateTimeProvider)
		{
			RuleFor(x => x.InitiatorId).NotEmpty();
			RuleFor(x => x.TeamId).NotEmpty();
			RuleFor(x => x.EventTypeId).NotEmpty();

			RuleFor(x => x.FromUtc)
				.NotEmpty()
				.GreaterThan(dateTimeProvider.UtcNow)
				.WithMessage("Cannot create event in past.");

			RuleFor(x => x.ToUtc)
				.NotEmpty()
				.Must((model, to) => model.FromUtc < to)
				.WithMessage("Event cannot end before it starts.");

			RuleFor(x => x.Description)
				.NotEmpty()
				.MaximumLength(EventConstants.EVENT_DESCRIPTION_MAX_SIZE);

			RuleFor(x => x.MeetTime).GreaterThan(TimeSpan.Zero);

			RuleFor(x => x.ReplyClosingTimeBeforeMeetTime).GreaterThan(TimeSpan.Zero);
		}
	}
}
