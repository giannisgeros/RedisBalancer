using RedisBalancer.Types;

namespace RedisBalancer.Utils
{
	interface IAddItemsUtils<TItem>
	{
		void AddItems(TItem[] items, IInstance<TItem>[] instances);
	}

	class AddItemsUtils<TItem> : IAddItemsUtils<TItem>
		where TItem : IEquatable<TItem>
	{
		private readonly IDistributeItemsUtils<TItem> _distributeItemsUtils;

		public AddItemsUtils(IDistributeItemsUtils<TItem> distributeItemsUtils)
		{
			_distributeItemsUtils = distributeItemsUtils;
		}

		public void AddItems(TItem[] items, IInstance<TItem>[] instances)
		{
			var existingItems = instances.SelectMany(x => x.Items).ToArray();

			var newItems = new List<TItem>();

			foreach (var item in items)
			{
				var existingItem = existingItems.FirstOrDefault(i => item.Equals(i));

				if (existingItem is null || existingItem.Equals(default))
					newItems.Add(item);
			}

			_distributeItemsUtils.DistributeItems(instances, newItems);
		}
	}
}
