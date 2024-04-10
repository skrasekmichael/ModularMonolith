using Microsoft.AspNetCore.Routing;

namespace TeamUp.Common.Endpoints;

public interface IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group);
}
