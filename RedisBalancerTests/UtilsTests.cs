using RedisBalancer.Types;
using RedisBalancer.Utils;

namespace RedisBalancerTests
{
	public class UtilsTests
	{
		[Fact]
		public void RemoveItems_WithOverlappingItems_ShouldReturnTheCommonSetOfItems()
		{
			// Arrange
			var removeItemsUtils = new RemoveItemsUtils<Item>();

			var items = Enumerable.Range(0, 5).Select(x => new Item(x)).ToArray();

			var now = DateTime.UtcNow;
			var instances = new IInstance<Item>[]
			{
				new Instance<Item>("instance-1", now, new List<Item> { new Item(0), new Item(6), new Item(7)}),
				new Instance<Item>("instance-2", now, new List<Item> { new Item(1), new Item(8)})
			};

			var expectedInstancesAfterRemoveItems = new IInstance<Item>[]
			{
				new Instance<Item>("instance-1", now, new List<Item> { new Item(0) }),
				new Instance<Item>("instance-2", now, new List<Item> { new Item(1) }),
			};

			// Act
			removeItemsUtils.RemoveItems(items, instances);

			// Assert
			Assert.Equal(instances.First().Items, expectedInstancesAfterRemoveItems.First().Items);
			Assert.Equal(instances.Last().Items, expectedInstancesAfterRemoveItems.Last().Items);
		}

		[Fact]
		public void DistributeItems_WithEmptyInstances_ShouldReturnBalancedInstances()
		{
			// Arrange
			var distributeItemsUtils = new DistributeItemsUtils<Item>();

			var items = Enumerable.Range(0, 5).Select(x => new Item(x)).ToList();

			var now = DateTime.UtcNow;
			var instances = new IInstance<Item>[]
			{
				new Instance<Item>("instance-1", now, new List<Item>()),
				new Instance<Item>("instance-2", now, new List<Item>())
			};

			var expectedInstancesAfterRemoveItems = new IInstance<Item>[]
			{
				new Instance<Item>("instance-1", now, new List<Item> { new Item(0), new Item(2), new Item(4)}),
				new Instance<Item>("instance-2", now, new List<Item> { new Item(1), new Item(3)})
			};

			// Act
			distributeItemsUtils.DistributeItems(instances, items);

			// Assert
			Assert.Equal(instances.First().Items, expectedInstancesAfterRemoveItems.First().Items);
			Assert.Equal(instances.Last().Items, expectedInstancesAfterRemoveItems.Last().Items);
		}

		[Fact]
		public void AddItems_WithOverlappingItems_ShouldBalanceTheItemsWithoutDuplicates()
		{
			// Arrange
			var distributeItemsUtils = new DistributeItemsUtils<Item>();
			var addItemsUtils = new AddItemsUtils<Item>(distributeItemsUtils);

			var items = Enumerable.Range(0, 5).Select(x => new Item(x)).ToArray();

			var now = DateTime.UtcNow;
			var instances = new IInstance<Item>[]
			{
				new Instance<Item>("instance-1", now, new List<Item> {new Item(0) }),
				new Instance<Item>("instance-2", now, new List<Item>())
			};

			var expectedInstancesAfterRemoveItems = new IInstance<Item>[]
			{
				new Instance<Item>("instance-1", now, new List<Item> { new Item(0), new Item(2), new Item(4)}),
				new Instance<Item>("instance-2", now, new List<Item> { new Item(1), new Item(3)})
			};

			// Act
			addItemsUtils.AddItems(items, instances);

			// Assert
			Assert.Equal(instances.First().Items, expectedInstancesAfterRemoveItems.First().Items);
			Assert.Equal(instances.Last().Items, expectedInstancesAfterRemoveItems.Last().Items);
		}
	}
}
