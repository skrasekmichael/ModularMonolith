using System.ComponentModel.DataAnnotations;

using TeamUp.TeamManagement.Contracts.Teams;

namespace TeamUp.TeamManagement.Contracts.Events.CreateEvent;

public sealed record CreateEventRequest
{
	public required EventTypeId EventTypeId { get; init; }

	[DataType(DataType.DateTime)]
	public required DateTime FromUtc { get; init; }

	[DataType(DataType.DateTime)]
	public required DateTime ToUtc { get; init; }

	[DataType(DataType.Text)]
	public required string Description { get; init; }

	[DataType(DataType.Time)]
	public required TimeSpan MeetTime { get; init; }

	[DataType(DataType.Time)]
	public required TimeSpan ReplyClosingTimeBeforeMeetTime { get; init; }
}
