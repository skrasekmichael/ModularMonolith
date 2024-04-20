using System.Reflection;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.Common.Domain;
using TeamUp.Common.Endpoints;
using TeamUp.Common.Infrastructure.Modules;
using TeamUp.Common.Infrastructure.Processing;
using TeamUp.Tests.Architecture.Extensions;
using TeamUp.Tests.Common;

namespace TeamUp.Tests.Architecture;

public abstract class BaseArchitectureTests
{
	public const string APPLICATION_LAYER = ".Application";
	public const string CONTRACTS_LAYER = ".Contracts";
	public const string INFRASTRUCTURE_LAYER = ".Infrastructure";
	public const string DOMAIN_LAYER = ".Domain";
	public const string ENDPOINTS_LAYER = ".Endpoints";

	public static readonly Assembly BootstrapperAssembly = typeof(Program).Assembly;

	public static readonly Assembly CommonApplicationAssembly = typeof(IIntegrationEventHandler<>).Assembly;
	public static readonly Assembly CommonContractsAssembly = typeof(IIntegrationEvent).Assembly;
	public static readonly Assembly CommonDomainAssembly = typeof(IDomainEvent).Assembly;
	public static readonly Assembly CommonEndpointsAssembly = typeof(IEndpointGroup).Assembly;
	public static readonly Assembly CommonInfrastructureAssembly = typeof(IModule).Assembly;

	public static readonly Assembly[] CommonAssemblies = [
		CommonApplicationAssembly,
		CommonContractsAssembly,
		CommonDomainAssembly,
		CommonEndpointsAssembly,
		CommonInfrastructureAssembly
	];

	public static IModule[] Modules => ModulesAccessor.Modules;

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
