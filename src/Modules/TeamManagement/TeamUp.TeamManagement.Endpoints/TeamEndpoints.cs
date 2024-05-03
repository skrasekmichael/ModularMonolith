using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Endpoints.Teams;

namespace TeamUp.TeamManagement.Endpoints;

internal sealed class TeamEndpoints : IEndpointGroup
{
	public EndpointGroupBuilder MapEndpoints(EndpointGroupBuilder group)
	{
		return group.CreateGroup("teams", group =>
		{
			group
				.AddEndpoint<CreateTeamEndpoint>()
				.AddEndpoint<GetTeamEndpoint>()
				.AddEndpoint<GetUserTeamsEndpoint>()
				.AddEndpoint<DeleteTeamEndpoint>()
				.AddEndpoint<ChangeOwnershipEndpoint>()
				.AddEndpoint<RemoveTeamMemberEndpoint>()
				.AddEndpoint<UpdateTeamMemberRoleEndpoint>()
				.AddEndpoint<UpdateTeamNameEndpoint>()
				.AddEndpoint<ChangeNicknameEndpoint>()
				.AddEndpoint<CreateEventTypeEndpoint>()
				.MapGroup<EventsEndpointGroup>();
		});
	}
}
