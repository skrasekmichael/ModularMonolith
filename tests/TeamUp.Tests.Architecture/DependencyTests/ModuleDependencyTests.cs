using System.Reflection;
using System.Text.RegularExpressions;

using FluentAssertions;

using NetArchTest.Rules;

using TeamUp.Common.Infrastructure;
using TeamUp.Tests.Architecture.Extensions;

namespace TeamUp.Tests.Architecture.DependencyTests;

public sealed class ModuleDependencyTests : BaseArchitectureTests
{
	public static TheoryData<BaseModule> ModulesData => new(Modules);

	private static readonly string[] AllowedLayerNames = [
		ENDPOINTS_LAYER,
		APPLICATION_LAYER,
		DOMAIN_LAYER,
		INFRASTRUCTURE_LAYER,
		CONTRACTS_LAYER
	];

	//TeamUp.ModuleName.LayerName
	private static readonly Regex ModuleAssemblyNameRegex = new($"TeamUp.[a-zA-Z]+({string.Join('|', AllowedLayerNames)})");

	[Theory]
	[MemberData(nameof(ModulesData))]
	public void EachModulesLayer_Should_FollowNamingConvention(BaseModule module)
	{
		var failingLayers = new List<Assembly>();

		foreach (var layerAssembly in module.Assemblies)
		{
			var name = layerAssembly.GetName().Name!;
			if (!ModuleAssemblyNameRegex.IsMatch(name))
			{
				failingLayers.Add(layerAssembly);
			}
		}

		failingLayers.Should().BeEmpty();
	}

	[Theory]
	[MemberData(nameof(ModulesData))]
	public void EachModulesApplicationLayer_Should_DependOnlyOn_CommonApplication_Or_Domain_Or_ContractsOfModules(BaseModule module)
	{
		var assemblies = GetAllAssemblies();
		var applicationAssembly = module.GetLayer(APPLICATION_LAYER);

		var allowedAssemblies = new List<Assembly>()
		{
			applicationAssembly,
			CommonApplicationAssembly,
			module.GetLayer(DOMAIN_LAYER)
		};
		allowedAssemblies.AddRange(Modules.GetLayerAssemblies(CONTRACTS_LAYER));

		var failingTypes = Types.InAssembly(applicationAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except(allowedAssemblies).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}

	[Theory]
	[MemberData(nameof(ModulesData))]
	public void EachModulesDomainLayer_Should_DependOnlyOn_CommonDomain_Or_ContractsOfModules(BaseModule module)
	{
		var assemblies = GetAllAssemblies();
		var domainAssembly = module.GetLayer(DOMAIN_LAYER);

		var allowedAssemblies = new List<Assembly>()
		{
			domainAssembly,
			CommonDomainAssembly
		};
		allowedAssemblies.AddRange(Modules.GetLayerAssemblies(CONTRACTS_LAYER));

		var failingTypes = Types.InAssembly(domainAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except(allowedAssemblies).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}

	[Theory]
	[MemberData(nameof(ModulesData))]
	public void EachModulesContractsLayer_Should_DependOnlyOn_CommonContracts(BaseModule module)
	{
		var assemblies = GetAllAssemblies();
		var contractsAssembly = module.GetLayer(CONTRACTS_LAYER);

		var allowedAssemblies = new Assembly[]
		{
			contractsAssembly,
			CommonContractsAssembly
		};

		var failingTypes = Types.InAssembly(contractsAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except(allowedAssemblies).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}

	[Theory]
	[MemberData(nameof(ModulesData))]
	public void EachModulesInfrastructureLayer_Should_DependOnlyOn_CommonInfrastructure_Or_Domain_Or_Application(BaseModule module)
	{
		var assemblies = GetAllAssemblies();
		var infrastructureAssembly = module.GetLayer(INFRASTRUCTURE_LAYER);

		var allowedAssemblies = new Assembly[]
		{
			infrastructureAssembly,
			CommonInfrastructureAssembly,
			module.GetLayer(DOMAIN_LAYER),
			module.GetLayer(APPLICATION_LAYER)
		};

		var failingTypes = Types.InAssembly(infrastructureAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except(allowedAssemblies).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}

	[Theory]
	[MemberData(nameof(ModulesData))]
	public void EachModulesEndpointsLayer_Should_DependOnlyOn_CommonEndpoints_Or_Contracts(BaseModule module)
	{
		var assemblies = GetAllAssemblies();
		var endpointsAssembly = module.GetLayer(ENDPOINTS_LAYER);

		var allowedAssemblies = new Assembly[]
		{
			endpointsAssembly,
			CommonEndpointsAssembly,
			module.GetLayer(CONTRACTS_LAYER)
		};

		var failingTypes = Types.InAssembly(endpointsAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except(allowedAssemblies).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}
}
