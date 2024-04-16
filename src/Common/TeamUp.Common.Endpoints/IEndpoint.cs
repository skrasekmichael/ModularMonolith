using Microsoft.AspNetCore.Routing;

namespace TeamUp.Common.Endpoints;

public interface IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder group);
}
