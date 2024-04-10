namespace TeamUp.UserAccess.Domain;

public sealed class Password : IEquatable<Password>
{
	public const int SALT_SIZE = 16; //128 bit salt
	public const int HASH_SIZE = 64; //512 bit hash
	public const int TOTAL_SIZE = SALT_SIZE + HASH_SIZE;

	public byte[] Salt { get; private set; }
	public byte[] Hash { get; private set; }

	public Password()
	{
		Salt = new byte[SALT_SIZE];
		Hash = new byte[HASH_SIZE];
	}

	public Password(byte[] salt, byte[] hash)
	{
		if (salt.Length != SALT_SIZE)
			throw new ArgumentException($"Received {salt.Length} bytes, {SALT_SIZE} was expected.", nameof(salt));
		else if (hash.Length != HASH_SIZE)
			throw new ArgumentException($"Received {hash.Length} bytes, {HASH_SIZE} was expected.", nameof(hash));

		Salt = salt;
		Hash = hash;
	}

	public Password(byte[] bytes)
	{
		if (bytes.Length != TOTAL_SIZE)
			throw new ArgumentException($"Received {bytes.Length} bytes, {TOTAL_SIZE} was expected.", nameof(bytes));

		Salt = bytes[..SALT_SIZE];
		Hash = bytes[SALT_SIZE..];
	}

	public byte[] GetBytes()
	{
		var bytes = new byte[TOTAL_SIZE];
		Salt.CopyTo(bytes, 0);
		Hash.CopyTo(bytes, SALT_SIZE);
		return bytes;
	}

	public bool Equals(Password? other) => other is Password password && Compare(this, password);

	public override bool Equals(object? obj) => obj is Password password && Compare(this, password);

	public override int GetHashCode() => HashCode.Combine(Salt, Hash);

	private static bool Compare(Password left, Password right) =>
		left.Salt.SequenceEqual(right.Salt) &&
		left.Hash.SequenceEqual(right.Hash);

	public override string ToString() => $"{Convert.ToHexString(Salt)}|{Convert.ToHexString(Hash)}";
}
