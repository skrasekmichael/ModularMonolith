namespace TeamUp.Bootstrapper.Middlewares;

public sealed class RequestLoggingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<RequestLoggingMiddleware> _logger;

	public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task Invoke(HttpContext context)
	{
		var headers = string.Join("\n\t", context.Request.Headers.Select(header => $"Header: {header.Key} - {header.Value}"));

		_logger.LogInformation("Request: {method} {schema}://{host}{path} from {remoteHost} {headers}", context.Request.Method, context.Request.Scheme, context.Request.Host, context.Request.Path, context.Connection.RemoteIpAddress, headers);

		await _next(context);
	}
}
