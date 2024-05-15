using TeamUp.UserAccess.Contracts;

namespace TeamUp.UserAccess.Domain;

public interface IClientUrlGenerator
{
	public string GetActivationUrl(UserId userId);
	public string GetCompleteAccountRegistrationUrl(UserId userId);
}
