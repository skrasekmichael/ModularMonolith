using Microsoft.AspNetCore.Builder;

using TeamUp.Common.Endpoints;

namespace TeamUp.TeamManagement.Endpoints;

public sealed class TeamManagementEndpointGroup : IEndpointGroup
{
	public EndpointGroupBuilder MapEndpoints(EndpointGroupBuilder group)
	{
		return group
			.MapGroup<TeamEndpoints>()
			.MapGroup<InvitationEndpoints>()
			.Configure(group => group.RequireAuthorization());
	}
}
