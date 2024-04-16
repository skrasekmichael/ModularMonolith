using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TeamUp.Common.Endpoints;

public abstract class EndpointGroup([StringSyntax("Route")] string prefix) : IEndpointGroup
{
	private readonly string _prefix = prefix;

	public abstract void Map(RouteGroupBuilder group);

	public RouteGroupBuilder MapEndpoints(RouteGroupBuilder apiGroup)
	{
		var group = apiGroup
			.MapGroup(_prefix)
			.WithTags(GetType().Name);

		Map(group);
		return group;
	}
}
