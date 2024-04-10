namespace TeamUp.Bootstrapper.Middlewares;

public sealed class ResponseLoggingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<RequestLoggingMiddleware> _logger;

	public ResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task Invoke(HttpContext context)
	{
		await _next(context);

		var headers = string.Join("\n\t", context.Response.Headers.Select(header => $"Header: {header.Key} - {header.Value}"));

		_logger.LogInformation("Response: {statusCode}{headers}", context.Response.StatusCode, headers);
	}
}
