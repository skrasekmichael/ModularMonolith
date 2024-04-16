using Microsoft.AspNetCore.Routing;

namespace TeamUp.Common.Endpoints;

public interface IEndpointGroup
{
	public RouteGroupBuilder MapEndpoints(RouteGroupBuilder group);
}
