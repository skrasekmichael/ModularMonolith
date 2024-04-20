using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TeamUp.Common.Endpoints;

public sealed class EndpointGroupBuilder
{
	public RouteGroupBuilder Group { get; private set; }

	private readonly List<EndpointGroupBuilder> _subGroups = [];

	public EndpointGroupBuilder(RouteGroupBuilder group)
	{
		Group = group;
	}

	public EndpointGroupBuilder WithTags(params string[] tags)
	{
		foreach (var group in _subGroups)
		{
			group.Group.WithTags(tags);
		}

		return this;
	}

	public EndpointGroupBuilder AddEndpoint<TEndpoint>() where TEndpoint : IEndpoint, new()
	{
		var endpoint = new TEndpoint();
		endpoint.MapEndpoint(Group);
		return this;
	}

	public EndpointGroupBuilder Configure(Func<RouteGroupBuilder, RouteGroupBuilder> configureGroup)
	{
		Group = configureGroup(Group);
		return this;
	}

	public EndpointGroupBuilder CreateGroup([StringSyntax("Route")] string prefix, Action<EndpointGroupBuilder> configureGroup)
	{
		var newGroup = Group.MapGroup(prefix);

		var newEndpointGroup = new EndpointGroupBuilder(newGroup);
		configureGroup(newEndpointGroup);
		_subGroups.Add(newEndpointGroup);

		return this;
	}

	public EndpointGroupBuilder MapGroup<TEndpointGroup>() where TEndpointGroup : IEndpointGroup, new()
	{
		var group = new TEndpointGroup();
		group.MapEndpoints(this);

		return this;
	}
}
