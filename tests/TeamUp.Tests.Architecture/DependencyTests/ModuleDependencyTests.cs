using System.Reflection;
using System.Text.RegularExpressions;

using FluentAssertions;

using NetArchTest.Rules;

using TeamUp.Common.Infrastructure.Modules;
using TeamUp.Tests.Architecture.Extensions;
using TeamUp.Tests.Common.Extensions;

namespace TeamUp.Tests.Architecture.DependencyTests;

public sealed class ModuleDependencyTests : BaseArchitectureTests
{
	public static TheoryData<IModule> ModulesData => new(Modules);

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
	public void EachModulesLayer_Should_FollowNamingConvention(IModule module)
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
	public void EachModulesApplicationLayer_Should_DependOnlyOn_CommonApplication_Or_CommonDomain_Or_Domain_Or_ContractsOfModules(IModule module)
	{
		var assemblies = GetAllAssemblies();
		var applicationAssembly = module.GetLayer(APPLICATION_LAYER);

		var allowedAssemblies = Modules.GetLayerAssemblies(CONTRACTS_LAYER).With(
			applicationAssembly,
			CommonDomainAssembly,
			CommonApplicationAssembly,
			module.GetLayer(DOMAIN_LAYER)
		);

		var failingTypes = Types.InAssembly(applicationAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except(allowedAssemblies).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}

	[Theory]
	[MemberData(nameof(ModulesData))]
	public void EachModulesDomainLayer_Should_DependOnlyOn_CommonDomain_Or_CommonContracts_Or_ContractsOfModules(IModule module)
	{
		var assemblies = GetAllAssemblies();
		var domainAssembly = module.GetLayer(DOMAIN_LAYER);
		if (domainAssembly is null)
		{
			return;
		}

		var allowedAssemblies = Modules.GetLayerAssemblies(CONTRACTS_LAYER).With(
			domainAssembly,
			CommonDomainAssembly,
			CommonContractsAssembly
		);

		var failingTypes = Types.InAssembly(domainAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except(allowedAssemblies).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}

	[Theory]
	[MemberData(nameof(ModulesData))]
	public void EachModulesContractsLayer_Should_DependOnlyOn_CommonContracts(IModule module)
	{
		var assemblies = GetAllAssemblies();
		var contractsAssembly = module.GetLayer(CONTRACTS_LAYER);

		var allowedAssemblies = contractsAssembly.With(CommonContractsAssembly);

		var failingTypes = Types.InAssembly(contractsAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except(allowedAssemblies).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}

	[Theory]
	[MemberData(nameof(ModulesData))]
	public void EachModulesInfrastructureLayer_Should_DependOnlyOn_CommonInfrastructure_Or_CommonDomain_Or_CommonContracts_Or_ModulesAssemblies_Or_ContractsOfModules(IModule module)
	{
		var assemblies = GetAllAssemblies();
		var infrastructureAssembly = module.GetLayer(INFRASTRUCTURE_LAYER);

		var allowedAssemblies = Modules.GetLayerAssemblies(CONTRACTS_LAYER).With(
			CommonInfrastructureAssembly,
			CommonDomainAssembly,
			CommonContractsAssembly
		).With(module.Assemblies);

		var failingTypes = Types.InAssembly(infrastructureAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except(allowedAssemblies).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}

	[Theory]
	[MemberData(nameof(ModulesData))]
	public void EachModulesEndpointsLayer_Should_DependOnlyOn_CommonEndpoints_Or_Contracts(IModule module)
	{
		var assemblies = GetAllAssemblies();
		var endpointsAssembly = module.GetLayer(ENDPOINTS_LAYER);
		if (endpointsAssembly is null)
		{
			return;
		}

		var allowedAssemblies = endpointsAssembly.With(
			endpointsAssembly,
			CommonEndpointsAssembly,
			module.GetLayer(CONTRACTS_LAYER)
		);

		var failingTypes = Types.InAssembly(endpointsAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except(allowedAssemblies).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}
}
