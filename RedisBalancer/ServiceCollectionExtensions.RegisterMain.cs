using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedisBalancer.Commands;
using RedisBalancer.Types;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace RedisBalancer
{
	public static partial class ServiceCollectionExtensions
	{
		private static void RegisterMain<TItem>(this IServiceCollection services, Func<IServiceProvider, IITemsProvider<TItem>> itemsProviderFactory, Func<IServiceProvider, ILogger>? loggerProviderFactory, ConnectionMultiplexer multiplexer) where TItem : IEquatable<TItem>
		{
			services.AddSingleton(serviceProvider =>
			{
				var itemsProvider = itemsProviderFactory(serviceProvider);

				var ping = serviceProvider.GetRequiredService<Ping<TItem>>();
				var removeInactiveInstances = serviceProvider.GetRequiredService<RemoveInactiveInstances<TItem>>();
				var distributeItems = serviceProvider.GetRequiredService<DistributeItems<TItem>>();

				var multiplexers = new RedLockMultiplexer[] { multiplexer };
				var lockFactory = RedLockFactory.Create(multiplexers);

				var options = serviceProvider.GetRequiredService<RedisBalancerOptions>();
				var logger = loggerProviderFactory is not null ? loggerProviderFactory(serviceProvider) : null;

				return new Main<TItem>(ping, removeInactiveInstances, distributeItems, lockFactory, options, logger);
			});

			services.AddHostedService(ctx => ctx.GetRequiredService<Main<TItem>>());
		}
	}
}
