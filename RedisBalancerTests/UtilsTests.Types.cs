namespace RedisBalancerTests
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
			if (other is null)
				return false;

			return Id == other.Id;
		}

		public override bool Equals(object? obj) 
			=> Equals(obj as Item);

		public override int GetHashCode()
			=> Id.GetHashCode();
	}
}
