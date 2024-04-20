namespace TeamUp.Tests.EndToEnd.Mocks;

internal sealed class CallbackCounter
{
	private readonly object _mutex = new();

	public int Count { get; private set; } = 0;

	private TaskCompletionSource<bool> _tcs = new();

	public void Invoke()
	{
		lock (_mutex)
		{
			Count++;
			_tcs.SetResult(true);
			_tcs = new();
		}
	}

	public void Reset()
	{
		lock (_mutex)
		{
			Count = 0;
			_tcs = new();
		}
	}

	public async Task WaitForCallbackAsync() => await _tcs.Task;
}
