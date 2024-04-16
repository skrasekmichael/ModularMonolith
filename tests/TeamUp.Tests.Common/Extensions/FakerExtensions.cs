using System.Linq.Expressions;
using System.Reflection;

using Bogus;

using FluentAssertions;

namespace TeamUp.Tests.Common.Extensions;


public static class FakerExtensions
{
	public static readonly Faker F = new();

	public static T PickRandomFromReadOnlyList<T>(this Faker faker, IReadOnlyList<T> list) where T : class
	{
		var index = faker.Random.Int(0, list.Count - 1);
		return list[index];
	}

	public static T PopRandom<T>(this Faker faker, List<T> list, int ignoreLastX = 0) where T : class
	{
		(list.Count - ignoreLastX).Should().BeGreaterThan(0);

		var index = faker.Random.Int(0, list.Count - ignoreLastX - 1);
		var elem = list[index];
		list.Remove(elem);
		return elem;
	}

	public static IList<TOut> Make<TOut, TListElem>(this Faker _, Func<List<TListElem>> prepareList, int count, Func<List<TListElem>, int, TOut> action)
	{
		var list = prepareList();
		return Enumerable.Range(1, count).Select((index, elem) => action(list, index)).ToList();
	}

	public static Faker<T> UsePrivateConstructor<T>(this Faker<T> faker) where T : class
		=> faker.CustomInstantiator(f => (Activator.CreateInstance(typeof(T), nonPublic: true) as T)!);

	public static Faker<T> RuleForBackingField<T, TProperty>(this Faker<T> faker, Expression<Func<T, TProperty>> property, TProperty value) where T : class
	{
		if (property.Body is not MemberExpression memberExpression || memberExpression.Member is not PropertyInfo propertyInfo)
		{
			throw new ArgumentException("Expression must be a MemberExpression pointing to a PropertyInfo", nameof(property));
		}

		var backingField = propertyInfo.Name.GetBackingField();
		return faker.RuleFor(backingField, _ => value);
	}

	public static Faker<T> RuleForBackingField<T, TProperty>(this Faker<T> faker, Expression<Func<T, TProperty>> property, Func<Faker, TProperty> setter) where T : class
	{
		if (property.Body is not MemberExpression memberExpression || memberExpression.Member is not PropertyInfo propertyInfo)
		{
			throw new ArgumentException("Expression must be a MemberExpression pointing to a PropertyInfo", nameof(property));
		}

		var backingField = propertyInfo.Name.GetBackingField();
		return faker.RuleFor(backingField, setter);
	}

	public static string GetBackingField(this string propertyName) => $"<{propertyName}>k__BackingField";
}
