namespace TeamUp.Tests.EndToEnd.Mocks;

internal sealed class Owner<TOwner, TService>(TService service)
{
	public TService Service { get; } = service;
}

internal sealed class Owner<TOwner, TSubOwner, TService>(TService service)
{
	public TService Service { get; } = service;
}
