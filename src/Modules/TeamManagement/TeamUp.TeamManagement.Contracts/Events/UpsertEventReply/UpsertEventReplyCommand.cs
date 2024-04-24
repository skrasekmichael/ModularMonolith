using FluentValidation;

using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Contracts.Events.UpsertEventReply;

public sealed record UpsertEventReplyCommand : ICommand
{
	public required UserId InitiatorId { get; init; }
	public required TeamId TeamId { get; init; }
	public required EventId EventId { get; init; }
	public required ReplyType ReplyType { get; init; }
	public required string Message { get; init; }

	public sealed class Validator : AbstractValidator<UpsertEventReplyCommand>
	{
		public Validator()
		{
			RuleFor(x => x.InitiatorId).NotEmpty();
			RuleFor(x => x.TeamId).NotEmpty();
			RuleFor(x => x.EventId).NotEmpty();

			RuleFor(x => x.ReplyType).IsInEnum();

			RuleFor(x => x.Message)
				.Empty().When(x => x.ReplyType == ReplyType.Yes, ApplyConditionTo.CurrentValidator)
				.NotEmpty().When(x => x.ReplyType == ReplyType.No || x.ReplyType == ReplyType.Maybe, ApplyConditionTo.CurrentValidator)
				.MaximumLength(EventConstants.EVENT_REPLY_MESSAGE_MAX_SIZE);
		}
	}
}
