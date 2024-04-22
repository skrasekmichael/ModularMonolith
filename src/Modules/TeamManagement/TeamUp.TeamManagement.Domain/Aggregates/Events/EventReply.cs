using TeamUp.Common.Domain;
using TeamUp.TeamManagement.Contracts.Events;

namespace TeamUp.TeamManagement.Domain.Aggregates.Events;

public sealed record EventReply : IValueObject
{
	private static readonly EventReply YesReply = new(ReplyType.Yes, string.Empty);

	public ReplyType Type { get; }
	public string Message { get; }

#pragma warning disable CS8618 // EF Core constructor
	private EventReply() { }
#pragma warning restore CS8618

	private EventReply(ReplyType type, string message)
	{
		Type = type;
		Message = message;
	}

	public static EventReply Yes() => YesReply;
	public static EventReply Delay(string message = "") => new(ReplyType.Delay, message);
	public static EventReply Maybe(string message) => new(ReplyType.Maybe, message);
	public static EventReply No(string message) => new(ReplyType.No, message);
}
