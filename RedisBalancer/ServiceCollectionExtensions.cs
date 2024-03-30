using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedisBalancer.Types;
using StackExchange.Redis;

namespace RedisBalancer
{
	public static partial class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRedisBalancer<TItem>(this IServiceCollection services, RedisBalancerOptions options, Func<IServiceProvider, IITemsProvider<TItem>> itemsProviderFactory, Func<IServiceProvider, ILogger>? loggerProviderFactory = null)
			where TItem : IEquatable<TItem>
		{
			services.AddSingleton(options);

			services.RegisterUtils<TItem>();

			var multiplexer = ConnectionMultiplexer.Connect(options.RedisConnectionString);

			services.RegisterRepositories<TItem>(multiplexer);

			services.RegisterCommands(itemsProviderFactory, loggerProviderFactory);

			services.RegisterQueries<TItem>();

			services.RegisterMain(itemsProviderFactory, loggerProviderFactory, multiplexer);

			return services;
		}
	}
}
