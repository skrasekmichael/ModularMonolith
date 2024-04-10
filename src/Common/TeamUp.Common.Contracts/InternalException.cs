namespace TeamUp.Common.Contracts;

public sealed class InternalException : Exception
{
	public InternalException(string message) : base(message) { }
}
