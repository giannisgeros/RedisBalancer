using RedisBalancer.Types;

namespace RedisBalancerExample
{
	public class ItemService : IITemsProvider<Item>
	{
		public Task<Item[]> GetAll()
		{
			var items = Enumerable
				.Range(0, new Random().Next(0, 10))
				.Select(x => new Item(x))
				.ToArray();

			return Task.FromResult(items);
		}
	}
}
