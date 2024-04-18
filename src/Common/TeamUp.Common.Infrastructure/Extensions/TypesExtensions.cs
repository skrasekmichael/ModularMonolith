namespace TeamUp.Common.Infrastructure.Extensions;

internal static class TypesExtensions
{
	internal static Type? GetInterfaceWithGenericDefinition(this Type type, Type definitionType)
		=> type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == definitionType);

	internal static bool ImplementInterfaceOfType(this Type type, Type interfaceType)
		=> type.GetInterfaces().Any(i => i == interfaceType);

	internal static Type? GetGenericType(this Type type, int index = 0)
	{
		var genericArgs = type.GetGenericArguments();
		return index >= 0 && index < genericArgs.Length ? genericArgs[index] : null;
	}
}
