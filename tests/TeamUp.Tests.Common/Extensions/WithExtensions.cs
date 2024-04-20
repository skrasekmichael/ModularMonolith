namespace TeamUp.Tests.Common.Extensions;

public static class WithExtensions
{
	public static IEnumerable<T> With<T>(this IEnumerable<T> list, IEnumerable<T?> values)
	{
		return list.Concat(values.Where(v => v is not null)!);
	}

	public static IEnumerable<T> With<T>(this IEnumerable<T> list, params T?[] values)
	{
		return list.Concat(values.Where(v => v is not null)!);
	}

	public static IEnumerable<T> With<T>(this T? val, params T?[] values)
	{
		var list = values.Where(v => v is not null).ToList();
		if (val is not null)
		{
			list.Add(val);
		}

		return list!;
	}
}
