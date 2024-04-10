using System.Reflection;

using TeamUp.Common.Infrastructure;

namespace TeamUp.Tests.Architecture.Extensions;

public static class AssemblyExtensions
{
	public static IEnumerable<Assembly> GetAssemblies(this IEnumerable<BaseModule> modules) =>
		modules.SelectMany(module => module.Assemblies);

	public static IEnumerable<Assembly> GetLayerAssemblies(this IEnumerable<BaseModule> modules, string layer) =>
		modules.SelectMany(module => module.Assemblies.Where(assembly => assembly.GetName().Name?.EndsWith(layer) == true));

	public static Assembly GetLayer(this BaseModule module, string layer) =>
		module.Assemblies.Single(assembly => assembly.GetName().Name?.EndsWith(layer) == true);

	public static string[] ToNames(this IEnumerable<Assembly> assemblies) =>
		assemblies.Select(assembly => assembly.GetName().Name!).ToArray();
}
