using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Contracts;
using TeamUp.Common.Endpoints;
using TeamUp.Common.Infrastructure.Persistence;

namespace TeamUp.Common.Infrastructure.Modules;

public interface IModuleWithEndpoints : IModule
{
	public void MapEndpoints(RouteGroupBuilder group);
}

public abstract class ModuleWithEndpoints<TModuleId, TDatabaseContext, TEndpointGroup> : Module<TModuleId, TDatabaseContext>, IModuleWithEndpoints
	where TModuleId : class, IModuleId
	where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
	where TEndpointGroup : IEndpointGroup, new()
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		var moduleEndpointsBuilder = new EndpointGroupBuilder(group);
		moduleEndpointsBuilder
			.MapGroup<TEndpointGroup>()
			.WithTags(typeof(TEndpointGroup).Name);
	}
}
