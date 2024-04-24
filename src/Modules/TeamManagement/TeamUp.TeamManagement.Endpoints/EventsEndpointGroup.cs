using TeamUp.Common.Endpoints;
using TeamUp.TeamManagement.Endpoints.Events;

namespace TeamUp.TeamManagement.Endpoints;

internal sealed class EventsEndpointGroup : IEndpointGroup
{
	public EndpointGroupBuilder MapEndpoints(EndpointGroupBuilder group)
	{
		return group.CreateGroup("/{teamId:guid}/events", group =>
		{
			group
				.AddEndpoint<CreateEventEndpoint>()
				.AddEndpoint<GetEventEndpoint>()
				.AddEndpoint<GetEventsEndpoint>()
				.AddEndpoint<RemoveEventEndpoint>()
				.AddEndpoint<UpsertEventReplyEndpoint>();
		});
	}
}
