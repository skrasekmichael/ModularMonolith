using System.Reflection;

using TeamUp.Common.Infrastructure.Modules;

namespace TeamUp.Tests.Architecture.Extensions;

public static class AssemblyExtensions
{
	public static IEnumerable<Assembly> GetAssemblies(this IEnumerable<IModule> modules) =>
		modules.SelectMany(module => module.Assemblies);

	public static IEnumerable<Assembly> GetLayerAssemblies(this IEnumerable<IModule> modules, string layer) =>
		modules.SelectMany(module => module.Assemblies.Where(assembly => assembly.GetName().Name?.EndsWith(layer) == true));

	public static Assembly GetLayer(this IModule module, string layer) =>
		module.Assemblies.Single(assembly => assembly.GetName().Name?.EndsWith(layer) == true);

	public static string[] ToNames(this IEnumerable<Assembly> assemblies) =>
		assemblies.Select(assembly => assembly.GetName().Name!).ToArray();
}
