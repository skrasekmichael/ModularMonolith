namespace TeamUp.TeamManagement.Contracts.Events;

public readonly struct ReplyCountResponse
{
	public required ReplyType Type { get; init; }
	public required int Count { get; init; }
}
