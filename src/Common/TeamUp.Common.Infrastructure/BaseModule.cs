using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace TeamUp.Common.Infrastructure;

public abstract class BaseModule
{
	public abstract Assembly[] Assemblies { get; }

	public abstract void ConfigureService(IServiceCollection services);
	public virtual void ConfigureHealthChecks(IHealthChecksBuilder healthChecks) { }
}
