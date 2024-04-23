namespace TeamUp.Tests.EndToEnd.Mocks;

internal sealed class DelayedCommitUnitOfWorkOptions
{
	public bool IsDelayRequested { get; set; } = false;
}
