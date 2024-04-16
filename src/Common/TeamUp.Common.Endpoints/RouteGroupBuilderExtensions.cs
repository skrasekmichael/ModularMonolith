using Microsoft.AspNetCore.Routing;

namespace TeamUp.Common.Endpoints;

public static class RouteGroupBuilderExtensions
{
	public static RouteGroupBuilder MapEndpointGroup<TGroup>(this RouteGroupBuilder group) where TGroup : IEndpointGroup, new()
	{
		var groupEndpoints = new TGroup();
		groupEndpoints.MapEndpoints(group);
		return group;
	}

	public static RouteGroupBuilder MapEndpoint<TEndpoint>(this RouteGroupBuilder group) where TEndpoint : IEndpoint, new()
	{
		var endpoint = new TEndpoint();
		endpoint.MapEndpoint(group);
		return group;
	}
}
