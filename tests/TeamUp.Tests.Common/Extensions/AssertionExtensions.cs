using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using FluentAssertions.Execution;

using FluentAssertions.Primitives;

using FluentAssertions;

namespace TeamUp.Tests.Common.Extensions;

public static class AssertionExtensions
{
	public static AndConstraint<ObjectAssertions> ShouldNotBeNull<TObj>(
		[NotNull] this TObj? obj,
		[CallerArgumentExpression(nameof(obj))] string? callerName = null,
		string because = "",
		params object[] becauseArgs)
	{
		var assertion = new ObjectAssertions(obj);

		Execute.Assertion
			.ForCondition(assertion.Subject is not null)
			.BecauseOf(because, becauseArgs)
			.WithDefaultIdentifier(callerName)
			.FailWith("Expected {context} not to be <null>{reason}.");

#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
		return new AndConstraint<ObjectAssertions>(assertion);
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.
	}
}
