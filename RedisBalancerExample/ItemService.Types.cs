namespace RedisBalancerExample
{
	public class Item : IEquatable<Item>
	{
		public int Id { get; }

		public Item(int id)
		{
			Id = id;
		}

		public bool Equals(Item? other)
		{
			return other?.Id == Id;
		}
	}
}
