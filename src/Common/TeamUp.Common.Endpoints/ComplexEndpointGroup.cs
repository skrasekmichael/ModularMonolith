using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TeamUp.Common.Endpoints;

public abstract class ComplexEndpointGroup : IEndpointGroup
{
	public abstract IEndpointGroup[] GetEndpointGroups();

	public RouteGroupBuilder MapEndpoints(RouteGroupBuilder apiGroup)
	{
		var groups = GetEndpointGroups();

		foreach (var group in groups)
		{
			group.MapEndpoints(apiGroup).WithTags(GetType().Name);
		}

		return apiGroup;
	}
}
