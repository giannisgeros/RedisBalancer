using Microsoft.Extensions.DependencyInjection;
using RedisBalancer.RedisContext;
using RedisBalancer.Repositories;
using RedisBalancer.Types;
using StackExchange.Redis;

namespace RedisBalancer
{
	public static partial class ServiceCollectionExtensions
	{
		private static void RegisterRepositories<TItem>(this IServiceCollection services, ConnectionMultiplexer multiplexer) 
			where TItem : IEquatable<TItem>
		{
			services.AddSingleton<IRedisRepository<IInstance<TItem>>>(serviceProvider =>
			{
				var options = serviceProvider.GetRequiredService<RedisBalancerOptions>();

				return new RedisRepository<IInstance<TItem>>(multiplexer, options);
			});

			services.AddSingleton<IInstancesRepository<TItem>, InstancesRepository<TItem>>();
		}
	}
}
