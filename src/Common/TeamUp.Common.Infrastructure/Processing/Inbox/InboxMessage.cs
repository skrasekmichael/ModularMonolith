namespace TeamUp.Common.Infrastructure.Processing.Inbox;

internal sealed record InboxMessage
{
	public required Guid Id { get; init; }
	public required DateTime CreatedUtc { get; init; }
	public required string Assembly { get; init; }
	public required string Type { get; init; }
	public required string Data { get; init; }
	public DateTime? ProcessedUtc { get; set; } = null;
	public string? Error { get; set; } = null;
}
