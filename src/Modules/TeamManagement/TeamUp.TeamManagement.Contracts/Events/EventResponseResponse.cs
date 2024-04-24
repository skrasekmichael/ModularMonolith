using TeamUp.TeamManagement.Contracts.Teams;

namespace TeamUp.TeamManagement.Contracts.Events;

public sealed class EventResponseResponse
{
	public required TeamMemberId TeamMemberId { get; init; }
	public required string TeamMemberNickname { get; init; }
	public required ReplyType Type { get; init; }
	public required string Message { get; init; }
	public required DateTime TimeStampUtc { get; init; }
}
