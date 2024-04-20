using TeamUp.Common.Endpoints;

namespace TeamUp.TeamManagement.Endpoints;

public sealed class TeamEndpoints : IEndpointGroup
{
	public EndpointGroupBuilder MapEndpoints(EndpointGroupBuilder group)
	{
		return group;
	}
}
