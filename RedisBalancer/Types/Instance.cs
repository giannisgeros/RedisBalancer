namespace RedisBalancer.Types
{
	public interface IInstance<TItem>
	{
		string Name { get; }
		DateTime LastPing { get; }
		List<TItem> Items { get; }
		void Ping();
		void AddItem(TItem item);
		void RemoveItem(TItem item);
	}

	class Instance<TItem> : IInstance<TItem>
		where TItem : IEquatable<TItem>
	{
		public string Name { get; }
		public DateTime LastPing { get; private set; }
		public List<TItem> Items { get; }

		public Instance(string name, DateTime lastPing, List<TItem> items)
		{
			Name = name;
			LastPing = lastPing;
			Items = items;
		}

		public void Ping()
		{
			LastPing = DateTime.UtcNow;
		}

		public void AddItem(TItem item)
		{
			Items.Add(item);
		}

		public void RemoveItem(TItem item)
		{
			var existingItem = Items.FirstOrDefault(i => item.Equals(i));

			if (item is not null)
				Items.Remove(item);
		}
	}
}
