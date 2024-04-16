using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Application.Abstractions;

public interface IPasswordService
{
	public Password HashPassword(string password);
	public bool VerifyPassword(string inputRawPassword, Password dbPassword);
}
