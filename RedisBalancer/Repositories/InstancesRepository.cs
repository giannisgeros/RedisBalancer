using RedisBalancer.RedisContext;
using RedisBalancer.Types;

namespace RedisBalancer.Repositories
{
	interface IInstancesRepository<TItem>
	{
		Task<IInstance<TItem>[]> GetAll();
		Task<IInstance<TItem>> Get(string name);
		Task<IInstance<TItem>?> TryGet(string name);
		Task Add(IInstance<TItem> entity);
		Task Update(IInstance<TItem> entity);
		Task UpdateMany(IInstance<TItem>[] entities);
		Task RemoveMany(IInstance<TItem>[] entities);
	}

	class InstancesRepository<TItem> : IInstancesRepository<TItem>
	{
		private readonly string _redisKey;

		private readonly IRedisRepository<IInstance<TItem>> _redisRepository;

		public InstancesRepository(IRedisRepository<IInstance<TItem>> redisRepository, RedisBalancerOptions options)
		{
			_redisRepository = redisRepository;
			_redisKey = options.RedisKey;
		}

		public async Task<IInstance<TItem>[]> GetAll()
		{
			var entries = await _redisRepository.GetAll(_redisKey);

			return entries;
		}

		public async Task<IInstance<TItem>> Get(string name)
		{
			var entry = await _redisRepository.Get(_redisKey, entity => entity.Name == name);

			return entry;
		}

		public async Task<IInstance<TItem>?> TryGet(string name)
		{
			var entry = await _redisRepository.TryGet(_redisKey, entity => entity.Name == name);

			return entry;
		}

		public async Task Add(IInstance<TItem> instance)
		{
			await _redisRepository.Add(_redisKey, instance);
		}

		public async Task Update(IInstance<TItem> instance)
		{
			await _redisRepository.Update(_redisKey, entity => entity.Name == instance.Name, instance);
		}

		public async Task UpdateMany(IInstance<TItem>[] instances)
		{
			var names = instances.Select(instance => instance.Name).ToArray();

			await _redisRepository.UpdateMany(_redisKey, instance => names.Contains(instance.Name), instances);
		}

		public async Task RemoveMany(IInstance<TItem>[] instances)
		{
			var names = instances.Select(instance => instance.Name).ToArray();

			await _redisRepository.RemoveMany(_redisKey, instance => names.Contains(instance.Name), instances);
		}
	}
}
