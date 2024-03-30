using RedisBalancer.Types;

namespace RedisBalancer.Utils
{
	interface IDistributeItemsUtils<TItem>
	{
		void DistributeItems(IInstance<TItem>[] instances, List<TItem> items);
	}

	class DistributeItemsUtils<TItem> : IDistributeItemsUtils<TItem>
		where TItem : IEquatable<TItem>
	{
		public void DistributeItems(IInstance<TItem>[] instances, List<TItem> items)
		{
			if (!items.Any())
				return;

			var minimunLoadInstance = instances.OrderBy(x => x.Items.Count).First();

			minimunLoadInstance.AddItem(items[0]);

			items.RemoveAt(0);

			DistributeItems(instances, items);
		}
	}
}
