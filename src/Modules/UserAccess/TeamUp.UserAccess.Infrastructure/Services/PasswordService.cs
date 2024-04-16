using System.Security.Cryptography;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;

using TeamUp.UserAccess.Application.Abstractions;
using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Infrastructure.Services;

internal sealed class PasswordService : IPasswordService
{
	private readonly IOptions<UserAccessOptions> _options;

	public PasswordService(IOptions<UserAccessOptions> options)
	{
		_options = options;
	}

	public Password HashPassword(string password)
	{
		var salt = RandomNumberGenerator.GetBytes(Password.SALT_SIZE);
		var hash = HashPassword(salt, password, _options.Value.Hashing.Pepper);
		return new Password(salt, hash);
	}

	public bool VerifyPassword(string inputRawPassword, Password dbPassword)
	{
		var hash = HashPassword(dbPassword.Salt, inputRawPassword, _options.Value.Hashing.Pepper);
		return dbPassword.Hash.SequenceEqual(hash);
	}

	private byte[] HashPassword(byte[] salt, string password, string pepper)
	{
		return KeyDerivation.Pbkdf2(
			password: string.Concat(password, pepper),
			salt: salt,
			prf: KeyDerivationPrf.HMACSHA512,
			iterationCount: _options.Value.Hashing.Pbkdf2Iterations,
			numBytesRequested: Password.HASH_SIZE
		);
	}
}
