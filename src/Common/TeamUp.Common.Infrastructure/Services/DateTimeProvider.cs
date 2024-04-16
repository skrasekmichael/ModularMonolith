using TeamUp.Common.Contracts;

namespace TeamUp.Common.Infrastructure.Services;

internal sealed class DateTimeProvider : IDateTimeProvider
{
	public DateTime UtcNow => DateTime.UtcNow;

	public DateTimeOffset DateTimeOffsetUtcNow => DateTimeOffset.UtcNow;
}
