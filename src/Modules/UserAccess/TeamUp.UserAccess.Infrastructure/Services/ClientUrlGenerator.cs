using Microsoft.Extensions.Options;

using TeamUp.Common.Infrastructure.Options;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Infrastructure.Services;

internal sealed class ClientUrlGenerator : IClientUrlGenerator
{
	private readonly ClientOptions _options;

	public ClientUrlGenerator(IOptions<ClientOptions> options)
	{
		_options = options.Value;
	}

	public string GetActivationUrl(UserId userId) =>
		string.Format(_options.ActivateAccountUrl, _options.Url, userId.Value);

	public string GetCompleteAccountRegistrationUrl(UserId userId) =>
		string.Format(_options.CompleteAccountRegistrationUrl, _options.Url, userId.Value);
}
