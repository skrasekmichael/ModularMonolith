using System.Reflection;

using TeamUp.Common.Application;
using TeamUp.Common.Domain;
using TeamUp.Common.Endpoints;
using TeamUp.Common.Infrastructure;
using TeamUp.Domain.Abstractions;
using TeamUp.Tests.Architecture.Extensions;
using TeamUp.UserAccess.Infrastructure;

namespace TeamUp.Tests.Architecture;

public abstract class BaseArchitectureTests
{
	public const string APPLICATION_LAYER = ".Application";
	public const string CONTRACTS_LAYER = ".Contracts";
	public const string INFRASTRUCTURE_LAYER = ".Infrastructure";
	public const string DOMAIN_LAYER = ".Domain";
	public const string ENDPOINTS_LAYER = ".Endpoints";

	public static readonly Assembly BootstrapperAssembly = Assembly.GetAssembly(typeof(Program))!;

	public static readonly Assembly CommonApplicationAssembly = Assembly.GetAssembly(typeof(IIntegrationEventHandler))!;
	public static readonly Assembly CommonContractsAssembly = Assembly.GetAssembly(typeof(IIntegrationEvent))!;
	public static readonly Assembly CommonDomainAssembly = Assembly.GetAssembly(typeof(IDomainEvent))!;
	public static readonly Assembly CommonEndpointsAssembly = Assembly.GetAssembly(typeof(IEndpointGroup))!;
	public static readonly Assembly CommonInfrastructureAssembly = Assembly.GetAssembly(typeof(BaseModule))!;

	public static readonly Assembly[] CommonAssemblies = [
		CommonApplicationAssembly,
		CommonContractsAssembly,
		CommonDomainAssembly,
		CommonEndpointsAssembly,
		CommonInfrastructureAssembly
	];

	public static readonly BaseModule[] Modules = [
		new UserAccessModule()
	];

	public static Assembly[] GetAllAssemblies()
	{
		var moduleAssemblies = Modules.GetAssemblies().ToArray();
		var assemblies = new Assembly[moduleAssemblies.Length + CommonAssemblies.Length + 1];

		assemblies[0] = BootstrapperAssembly;
		moduleAssemblies.CopyTo(assemblies, 1);
		CommonAssemblies.CopyTo(assemblies, moduleAssemblies.Length + 1);

		return assemblies;
	}
}
