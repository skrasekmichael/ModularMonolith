namespace TeamUp.Common.Contracts;

public interface IDateTimeProvider
{
	public DateTime UtcNow { get; }

	public DateTimeOffset DateTimeOffsetUtcNow { get; }
}
