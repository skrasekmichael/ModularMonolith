using System.Security.Claims;

using Microsoft.AspNetCore.Http;

using TeamUp.Common.Contracts;

namespace TeamUp.Common.Endpoints;

public static class HttpContextExtensions
{
	public static TUserId GetCurrentUserId<TUserId>(this HttpContext httpContext)
		where TUserId : TypedId<TUserId>, new()
	{
		var guid = httpContext.ParseClaim(ClaimTypes.NameIdentifier, Guid.Parse);
		return TypedId<TUserId>.FromGuid(guid);
	}

	public static TOut ParseClaim<TOut>(this HttpContext httpContext, string type, Func<string, TOut> parse)
	{
		if (httpContext.User.Identity is ClaimsIdentity identity)
		{
			var claim = identity.Claims.Single(x => x.Type == type);
			return parse(claim.Value);
		}

		throw new InternalException($"Couldn't obtain '{type}' ClaimsIdentity from HttpContext.");
	}
}
