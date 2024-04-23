using Microsoft.Extensions.DependencyInjection;

namespace TeamUp.Tests.EndToEnd.Extensions;

public static class ServiceCollectionExtensions
{
	public static void Replace(this IServiceCollection services, Type serviceType, Type newServiceType)
	{
		var targetDescriptor = services.FirstOrDefault(x => x.ServiceType == serviceType);
		if (targetDescriptor is not null)
		{
			services.Remove(targetDescriptor);
			services.Add(new ServiceDescriptor(
				serviceType,
				newServiceType,
				targetDescriptor.Lifetime)
			);
		}
	}

	public static void Replace<TServiceType, TNewImplementationType>(this IServiceCollection services) where TNewImplementationType : class, TServiceType
	{
		var targetDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(TServiceType));
		if (targetDescriptor is not null)
		{
			services.Remove(targetDescriptor);
			services.Add(new ServiceDescriptor(
				typeof(TServiceType),
				typeof(TNewImplementationType),
				targetDescriptor.Lifetime)
			);
		}
	}

	public static void Replace<TServiceType, TNewImplementationType>(this IServiceCollection services, Func<IServiceProvider, TNewImplementationType> factory) where TNewImplementationType : class, TServiceType
	{
		var targetDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(TServiceType));
		if (targetDescriptor is not null)
		{
			services.Remove(targetDescriptor);
			services.Add(new ServiceDescriptor(
				typeof(TServiceType),
				factory,
				targetDescriptor.Lifetime)
			);
		}
	}
}
