namespace TeamUp.Tests.Common.Extensions;

public static class TimeExtensions
{
	//removes microseconds, is needed for testing as there is slight shift when saving to database
	public static DateTime DropMicroSeconds(this DateTime dateTime) => new(dateTime.Ticks / TimeSpan.TicksPerMillisecond * TimeSpan.TicksPerMillisecond, dateTime.Kind);
	public static TimeSpan DropMicroSeconds(this TimeSpan timespan) => new(timespan.Ticks / TimeSpan.TicksPerMillisecond * TimeSpan.TicksPerMillisecond);
}
