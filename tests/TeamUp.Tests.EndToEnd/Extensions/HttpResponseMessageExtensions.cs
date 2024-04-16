using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;

namespace TeamUp.Tests.EndToEnd.Extensions;

public static class HttpResponseMessageExtensions
{
	private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	public static Task<TOut?> ReadFromJsonAsync<TOut>(this HttpResponseMessage message, JsonSerializerOptions? options = null)
		=> message.Content.ReadFromJsonAsync<TOut>(options ?? DefaultJsonSerializerOptions);

	public static Task<ProblemDetails?> ReadProblemDetailsAsync(this HttpResponseMessage message, JsonSerializerOptions? options = null)
		=> message.Content.ReadFromJsonAsync<ProblemDetails>(options ?? DefaultJsonSerializerOptions);

	public static Task<ValidationProblemDetails?> ReadValidationProblemDetailsAsync(this HttpResponseMessage message, JsonSerializerOptions? options = null)
		=> message.Content.ReadFromJsonAsync<ValidationProblemDetails>(options ?? DefaultJsonSerializerOptions);
}
