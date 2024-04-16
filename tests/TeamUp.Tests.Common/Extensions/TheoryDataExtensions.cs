using System.Linq.Expressions;

using Xunit;

namespace TeamUp.Tests.Common.Extensions;

public static class TheoryDataExtensions
{
	public static void Add<TObject, TOut>(this TheoryData<InvalidRequest<TObject>> theoryData, Expression<Func<TObject, TOut>> property, TObject request)
	{
		var invalidObject = InvalidRequest<TObject>.Create(property, request);
		theoryData.Add(invalidObject);
	}
}
