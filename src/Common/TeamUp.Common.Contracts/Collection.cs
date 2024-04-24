using System.Collections;

namespace TeamUp.Common.Contracts;

public sealed class Collection<T> : IEnumerable<T>
{
	public IReadOnlyCollection<T> Values { get; }

	public Collection(IReadOnlyCollection<T> values)
	{
		Values = values;
	}

	public IEnumerator<T> GetEnumerator() => Values.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => Values.GetEnumerator();
}
