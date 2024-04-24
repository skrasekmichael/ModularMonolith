using System.Text.RegularExpressions;

namespace TeamUp.Common.Infrastructure.Extensions;

internal static partial class TypesExtensions
{
	internal static Type? GetInterfaceWithGenericDefinition(this Type type, Type definitionType)
		=> type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == definitionType);

	internal static bool ImplementInterfaceOfType(this Type type, Type interfaceType)
		=> type.GetInterfaces().Any(i => i == interfaceType);

	internal static Type? GetGenericType(this Type? type, int index = 0)
	{
		if (type is null)
		{
			return null;
		}

		var genericArgs = type.GetGenericArguments();
		return index >= 0 && index < genericArgs.Length ? genericArgs[index] : null;
	}

	[GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", RegexOptions.Compiled)]
	private static partial Regex AddDashBeforeCapitalLetterRegex();

	public static string? ToKebabCase(this Type? type)
	{
		if (type is null)
		{
			return null;
		}

		var pascalCase = type.FullName!.Replace(".", "");
		return AddDashBeforeCapitalLetterRegex().Replace(pascalCase, "-$1").ToLower();
	}
}
