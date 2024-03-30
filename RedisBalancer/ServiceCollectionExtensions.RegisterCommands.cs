using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedisBalancer.Commands;
using RedisBalancer.Repositories;
using RedisBalancer.Types;
using RedisBalancer.Utils;

namespace RedisBalancer
{
	public static partial class ServiceCollectionExtensions
	{
		private static void RegisterCommands<TItem>(this IServiceCollection services, Func<IServiceProvider, IITemsProvider<TItem>> itemsProviderFactory, Func<IServiceProvider, ILogger>? loggerProviderFactory) 
			where TItem : IEquatable<TItem>
		{
			services.AddSingleton(serviceProvider =>
			{
				var repository = serviceProvider.GetRequiredService<IInstancesRepository<TItem>>();
				var options = serviceProvider.GetRequiredService<RedisBalancerOptions>();
				var logger = loggerProviderFactory is not null ? loggerProviderFactory(serviceProvider) : null;

				return new Ping<TItem>(repository, options, logger);
			});

			services.AddSingleton(serviceProvider =>
			{
				var repository = serviceProvider.GetRequiredService<IInstancesRepository<TItem>>();
				var options = serviceProvider.GetRequiredService<RedisBalancerOptions>();
				var logger = loggerProviderFactory is not null ? loggerProviderFactory(serviceProvider) : null;

				return new RemoveInactiveInstances<TItem>(repository, options, logger);
			});

			services.AddSingleton(serviceProvider =>
			{
				var repository = serviceProvider.GetRequiredService<IInstancesRepository<TItem>>();
				var itemsProvider = itemsProviderFactory(serviceProvider);
				var addItemsUtils = serviceProvider.GetRequiredService<IAddItemsUtils<TItem>>();
				var removeItemsUtils = serviceProvider.GetRequiredService<IRemoveItemsUtils<TItem>>();
				var logger = loggerProviderFactory is not null ? loggerProviderFactory(serviceProvider) : null;

				return new DistributeItems<TItem>(repository, itemsProvider, addItemsUtils, removeItemsUtils, logger);
			});
		}
	}
}
