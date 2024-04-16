using System.Linq.Expressions;
using System.Reflection;

namespace TeamUp.Tests.Common;

public sealed class InvalidRequest<TRequest>
{
	public required string InvalidProperty { get; init; }
	public required TRequest Request { get; init; }

	public static InvalidRequest<TRequest> Create<TOut>(Expression<Func<TRequest, TOut>> property, TRequest request)
	{
		if (property.Body is not MemberExpression memberExpression || memberExpression.Member is not PropertyInfo propertyInfo)
		{
			throw new ArgumentException("Expression must be a MemberExpression pointing to a PropertyInfo", nameof(property));
		}

		return new()
		{
			InvalidProperty = propertyInfo.Name,
			Request = request
		};
	}
}
