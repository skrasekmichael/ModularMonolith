using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Application.Abstractions;

public interface ITokenService
{
	public string GenerateToken(User user);
}
