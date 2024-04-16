using Microsoft.AspNetCore.Routing;

using TeamUp.Common.Endpoints;

namespace TeamUp.UserAccess.Endpoints;

public sealed class UserAccessEndpointGroup() : EndpointGroup("users")
{
	public override void Map(RouteGroupBuilder group)
	{
		group.MapEndpoint<GetMyAccountEndpoint>()
			.MapEndpoint<RegisterUserEndpoint>()
			.MapEndpoint<LoginEndpoint>();
	}
}
