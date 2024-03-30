using RedisBalancer.Types;

namespace RedisBalancer.Utils
{
	interface IRemoveItemsUtils<TItem>
		where TItem : IEquatable<TItem>
	{
		void RemoveItems(TItem[] items, IInstance<TItem>[] instances);
	}

	class RemoveItemsUtils<TItem> : IRemoveItemsUtils<TItem>
		where TItem: IEquatable<TItem>
	{
		public void RemoveItems(TItem[] items, IInstance<TItem>[] instances)
		{
			foreach (var instance in instances)
			{
				var instanceItems = instance.Items.ToArray();

				foreach (var item in instanceItems)
				{
					var existingItem = items.FirstOrDefault(i => item.Equals(i));

					if (existingItem is null || existingItem.Equals(default))
						instance.RemoveItem(item);
				}
			}
		}
	}
}
