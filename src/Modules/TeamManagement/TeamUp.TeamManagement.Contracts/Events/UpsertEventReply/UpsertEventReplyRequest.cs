using System.ComponentModel.DataAnnotations;

namespace TeamUp.TeamManagement.Contracts.Events.UpsertEventReply;

public sealed record UpsertEventReplyRequest
{
	public required ReplyType ReplyType { get; init; }

	[DataType(DataType.Text)]
	public required string Message { get; init; }
}
