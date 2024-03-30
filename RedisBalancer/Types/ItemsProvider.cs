namespace RedisBalancer.Types
{
	public interface IITemsProvider<TItem>
		where TItem : IEquatable<TItem>
	{
		Task<TItem[]> GetAll();
	}
}
