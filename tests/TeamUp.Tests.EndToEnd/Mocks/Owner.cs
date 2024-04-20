namespace TeamUp.Tests.EndToEnd.Mocks;

internal sealed class Owner<TOwner, TService>(TService service)
{
	public TService Service { get; } = service;
}
