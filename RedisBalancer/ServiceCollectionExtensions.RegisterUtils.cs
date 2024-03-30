using Microsoft.Extensions.DependencyInjection;
using RedisBalancer.Utils;

namespace RedisBalancer
{
	public static partial class ServiceCollectionExtensions
	{
		private static void RegisterUtils<TItem>(this IServiceCollection services)
			where TItem : IEquatable<TItem>
		{
			var removeItemsUtls = new RemoveItemsUtils<TItem>();
			services.AddSingleton<IRemoveItemsUtils<TItem>>(removeItemsUtls);

			var distributeItemUtils = new DistributeItemsUtils<TItem>();
			services.AddSingleton<IDistributeItemsUtils<TItem>>(distributeItemUtils);

			var addItemsUtils = new AddItemsUtils<TItem>(distributeItemUtils);
			services.AddSingleton<IAddItemsUtils<TItem>>(addItemsUtils);
		}
	}
}
