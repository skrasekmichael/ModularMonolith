namespace TeamUp.Tests.EndToEnd.Mocks;

internal sealed class CallbackWithTimeout
{
	private readonly object _mutex = new();

	private TaskCompletionSource<bool> _tcs = new();

	public void Invoke()
	{
		lock (_mutex)
		{
			_tcs.SetResult(true);
			_tcs = new();
		}
	}

	public async Task<bool> WaitForCallbackAsync(int millisecondsTimeout = 2500)
	{
		var returnTask = await Task.WhenAny(_tcs.Task, Task.Delay(millisecondsTimeout));
		return returnTask == _tcs.Task;
	}
}
