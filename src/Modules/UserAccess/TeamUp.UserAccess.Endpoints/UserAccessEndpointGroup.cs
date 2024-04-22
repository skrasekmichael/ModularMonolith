using TeamUp.Common.Endpoints;

namespace TeamUp.UserAccess.Endpoints;

public sealed class UserAccessEndpointGroup : IEndpointGroup
{
	public EndpointGroupBuilder MapEndpoints(EndpointGroupBuilder group)
	{
		return group.CreateGroup("users", group =>
		{
			group
				.AddEndpoint<GetMyAccountEndpoint>()
				.AddEndpoint<RegisterUserEndpoint>()
				.AddEndpoint<LoginEndpoint>()
				.AddEndpoint<DeleteUserEndpoint>()
				.AddEndpoint<ActivateAccountEndpoint>();
		});
	}
}
