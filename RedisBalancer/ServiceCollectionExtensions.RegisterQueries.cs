using Microsoft.Extensions.DependencyInjection;
using RedisBalancer.Queries;

namespace RedisBalancer
{
	public static partial class ServiceCollectionExtensions
	{
		private static void RegisterQueries<TItem>(this IServiceCollection services) 
			where TItem : IEquatable<TItem>
		{
			services.AddSingleton<IGetInstances<TItem>, GetInstances<TItem>>();
		}
	}
}
