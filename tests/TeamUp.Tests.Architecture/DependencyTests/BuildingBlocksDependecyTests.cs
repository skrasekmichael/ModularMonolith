using System.Reflection;

using FluentAssertions;

using NetArchTest.Rules;

using TeamUp.Tests.Architecture.Extensions;

namespace TeamUp.Tests.Architecture.DependencyTests;

public sealed class BuildingBlocksDependencyTests : BaseArchitectureTests
{
	[Fact]
	public void Bootstrapper_Should_DependOnlyOn_ModulesContracts_And_ModulesEndpoints_And_ModulesInfrastructure_And_Common()
	{
		var assemblies = GetAllAssemblies();
		var allowedAssemblies = new List<Assembly>()
		{
			BootstrapperAssembly,
			CommonEndpointsAssembly,
			CommonInfrastructureAssembly
		};

		allowedAssemblies.AddRange(Modules.GetLayerAssemblies(CONTRACTS_LAYER));
		allowedAssemblies.AddRange(Modules.GetLayerAssemblies(ENDPOINTS_LAYER));
		allowedAssemblies.AddRange(Modules.GetLayerAssemblies(INFRASTRUCTURE_LAYER));

		var failingTypes = Types.InAssembly(BootstrapperAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except(allowedAssemblies).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}

	[Fact]
	public void CommonContracts_Should_HaveNoDependency()
	{
		var assemblies = GetAllAssemblies();

		var failingTypes = Types.InAssembly(CommonContractsAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except([CommonContractsAssembly]).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}

	[Fact]
	public void CommonInfrastructure_Should_DependOnlyOn_CommonLayers()
	{
		var assemblies = GetAllAssemblies();

		var allowedAssemblies = new List<Assembly>()
		{
			CommonDomainAssembly,
			CommonApplicationAssembly,
			CommonContractsAssembly,
			CommonInfrastructureAssembly,
			CommonEndpointsAssembly
		};

		var failingTypes = Types.InAssembly(CommonInfrastructureAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except(allowedAssemblies).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}

	[Fact]
	public void CommonEndpoints_Should_DependOnlyOn_CommonContracts()
	{
		var assemblies = GetAllAssemblies();

		var failingTypes = Types.InAssembly(CommonEndpointsAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except([CommonEndpointsAssembly, CommonContractsAssembly]).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}

	[Fact]
	public void CommonDomain_Should_DependOnlyOn_CommonContracts()
	{
		var assemblies = GetAllAssemblies();

		var failingTypes = Types.InAssembly(CommonDomainAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except([CommonDomainAssembly, CommonContractsAssembly]).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}

	[Fact]
	public void CommonApplication_Should_DependOnlyOn_CommonContracts()
	{
		var assemblies = GetAllAssemblies();

		var failingTypes = Types.InAssembly(CommonApplicationAssembly)
			.That()
			.HaveDependencyOnAny(assemblies.Except([CommonApplicationAssembly, CommonContractsAssembly]).ToNames())
			.GetTypes();

		failingTypes.Should().BeEmpty();
	}
}
