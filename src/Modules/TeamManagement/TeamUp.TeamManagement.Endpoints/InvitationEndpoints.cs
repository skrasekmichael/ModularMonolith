using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Endpoints.Invitations;

namespace TeamUp.TeamManagement.Endpoints;

internal sealed class InvitationEndpoints : IEndpointGroup
{
	public EndpointGroupBuilder MapEndpoints(EndpointGroupBuilder group)
	{
		return group.CreateGroup("invitations", group =>
		{
			group
				.AddEndpoint<InviteUserEndpoint>()
				.AddEndpoint<GetTeamInvitationsEndpoint>()
				.AddEndpoint<AcceptInvitationEndpoint>()
				.AddEndpoint<GetMyInvitationsEndpoint>()
				.AddEndpoint<RemoveInvitationEndpoint>();
		});
	}
}
