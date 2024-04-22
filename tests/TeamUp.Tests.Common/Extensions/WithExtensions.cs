namespace TeamUp.Tests.Common.Extensions;

public static class WithExtensions
{
	public static TResult Map<T, TResult>(this T source, Func<T, TResult> transform) => transform(source);

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
		if (val is null)
		{
			return list!;
		}

		list.Insert(0, val);
		return list!;
	}

	public static IEnumerable<T> With<T>(this T? val, IEnumerable<T> values)
	{
		var list = values.Where(v => v is not null).ToList();
		if (val is null)
		{
			return list!;
		}

		list.Insert(0, val);
		return list!;
	}

	public static List<T> Without<T>(this IEnumerable<T> list, T element)
	{
		var newList = new List<T>(list);
		newList.Remove(element);
		return newList;
	}
}
