namespace TeamUp.Common.Endpoints;

public interface IEndpointGroup
{
	public EndpointGroupBuilder MapEndpoints(EndpointGroupBuilder group);
}
